using System.Reflection;
using System.Text;
using Innowise.Clinic.Auth.Configuration.Swagger;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Middleware;
using Innowise.Clinic.Auth.Persistence;
using Innowise.Clinic.Auth.Services.AccountBlockingService.Implementations;
using Innowise.Clinic.Auth.Services.AccountBlockingService.Interfaces;
using Innowise.Clinic.Auth.Services.JwtService.Data;
using Innowise.Clinic.Auth.Services.JwtService.Implementations;
using Innowise.Clinic.Auth.Services.JwtService.Interfaces;
using Innowise.Clinic.Auth.Services.MassTransitService.Consumers;
using Innowise.Clinic.Auth.Services.RabbitMqConsumer;
using Innowise.Clinic.Auth.Services.RabbitMqConsumer.Options;
using Innowise.Clinic.Auth.Services.UserCredentialsGenerationService.Implementations;
using Innowise.Clinic.Auth.Services.UserCredentialsGenerationService.Interfaces;
using Innowise.Clinic.Auth.Services.UserManagementService.Data;
using Innowise.Clinic.Auth.Services.UserManagementService.Implementations;
using Innowise.Clinic.Auth.Services.UserManagementService.Interfaces;
using Innowise.Clinic.Auth.Validators.Custom;
using Innowise.Clinic.Auth.Validators.Identity;
using Innowise.Clinic.Auth.Validators.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Swashbuckle.AspNetCore.Filters;

namespace Innowise.Clinic.Auth.Configuration;

public static class ConfigurationExtensions
{
    public static IServiceCollection ConfigureSecurity(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtConfiguration = configuration.GetSection("JWT");
        var jwtValidationConfiguration = configuration.GetSection("JwtValidationConfiguration");
        var authenticationConfiguration = configuration.GetSection("AuthenticationRequirements");

        services.AddAuthentication();
        services.ConfigureIdentity(authenticationConfiguration);
        services.ConfigureJwtAuthentication(jwtConfiguration, jwtValidationConfiguration);
        services.ConfigureCustomValidators();
        services.AddSingleton<AuthenticationExceptionHandlingMiddleware>();
        return services;
    }

    public static IServiceCollection ConfigureUserManagementServices(this IServiceCollection services)
    {
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IUserCredentialsGenerationService, UserCredentialsGenerationService>();
        services.AddScoped<IAccountBlockingService, AccountBlockingService>();
        return services;
    }

    public static IServiceCollection ConfigureCrossServiceCommunication(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitOptions>(configuration.GetSection("RabbitConfigurations"));
        var rabbitMqConfig = configuration.GetSection("RabbitConfigurations");

        services.AddMassTransit(x =>
        {
            x.AddConsumer<PatientProfileCreatedMessageConsumer>();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqConfig["HostName"], h =>
                {
                    h.Username(rabbitMqConfig["UserName"]);
                    h.Password(rabbitMqConfig["Password"]);
                });
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddHostedService<RabbitMqConsumer>();
        return services;
    }

    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" }
                    },
                    new string[] { }
                }
            });

            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
                "Innowise.Clinic.Auth.Api.xml"));
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
                $"{Assembly.GetAssembly(typeof(AuthTokenPairDto))?.GetName().Name}.xml"));
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
                $"{Assembly.GetAssembly(typeof(UserCredentialsDto))?.GetName().Name}.xml"));
            options.ExampleFilters();
        });

        services.AddSwaggerExamplesFromAssemblyOf<SignInCredentialsExample>();
        return services;
    }

    public static IServiceCollection AddConfigurationOptions(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtConfiguration = configuration.GetSection("JWT");
        var jwtValidationConfiguration = configuration.GetSection("JwtValidationConfiguration");
        var authenticationRequirementsConfiguration = configuration.GetSection("AuthenticationRequirements");

        services.Configure<AuthenticationRequirementsSettings>(authenticationRequirementsConfiguration);
        services.Configure<JwtSettings>(jwtConfiguration);
        services.Configure<JwtValidationSettings>(jwtValidationConfiguration);
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        return services;
    }

    public static async Task PrepareDatabase(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ClinicAuthDbContext>();
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }

            var userManager = services.GetRequiredService<UserManager<IdentityUser<Guid>>>();
            await new DataSeeder(userManager).SeedUsers();
        }
    }

    private static IServiceCollection ConfigureIdentity(this IServiceCollection services,
        IConfigurationSection passwordValidationConfiguration)
    {
        services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit =
                    Convert.ToBoolean(passwordValidationConfiguration["RequireDigitsInPassword"]);
                options.Password.RequiredLength =
                    Convert.ToInt32(passwordValidationConfiguration["MinimalPasswordLength"]);
                options.Password.RequireLowercase =
                    Convert.ToBoolean(passwordValidationConfiguration["RequireLowercaseLettersInPassword"]);
                options.Password.RequireNonAlphanumeric =
                    Convert.ToBoolean(passwordValidationConfiguration["RequireNonAlphanumericInPassword"]);
                options.Password.RequireUppercase =
                    Convert.ToBoolean(passwordValidationConfiguration["RequireUppercaseLettersInPassword"]);
            })
            .AddEntityFrameworkStores<ClinicAuthDbContext>()
            .AddDefaultTokenProviders()
            .AddPasswordValidators();
        return services;
    }

    private static IdentityBuilder AddPasswordValidators(this IdentityBuilder builder)
    {
        builder.AddPasswordValidator<MaximalPasswordLengthValidator<IdentityUser<Guid>>>();
        return builder;
    }

    private static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services,
        IConfigurationSection jwtConfiguration, IConfigurationSection jwtValidationConfiguration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = Convert.ToBoolean(jwtValidationConfiguration["ValidateIssuer"]),
                ValidIssuer = jwtConfiguration["ValidIssuer"],
                ValidateAudience = Convert.ToBoolean(jwtValidationConfiguration["ValidateAudience"]),
                ValidAudience = jwtConfiguration["ValidAudience"],
                ValidateIssuerSigningKey = Convert.ToBoolean(jwtValidationConfiguration["ValidateIssuerSigningKey"]),
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration["Key"]))
            };
        });

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITokenRevoker, RefreshTokenRevoker>();
        services.AddScoped<ITokenStateValidator, TokenStateValidator>();
        return services;
    }

    private static IServiceCollection ConfigureCustomValidators(this IServiceCollection services)
    {
        services.AddScoped<ITokenValidator, TokenValidator>();
        services.AddScoped<IEmailValidator, EmailUniquenessValidator>();
        return services;
    }

    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration["ElasticSearchHost"]))
            {
                AutoRegisterTemplate = true,
                OverwriteTemplate = true,
                IndexFormat = $"clinic.authentication-{0:yy.MM}",
                BatchAction = ElasticOpType.Index,
                DetectElasticsearchVersion = true,
            })
            .WriteTo.Console()
            .CreateLogger();

        Log.Logger = logger;
        builder.Host.UseSerilog(logger);
        return builder;
    }
}