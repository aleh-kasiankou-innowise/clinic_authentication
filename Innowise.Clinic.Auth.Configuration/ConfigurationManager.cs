using System.Reflection;
using System.Text;
using Innowise.Clinic.Auth.Configuration.Swagger;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Middleware;
using Innowise.Clinic.Auth.Persistence;
using Innowise.Clinic.Auth.Services.JwtService.Data;
using Innowise.Clinic.Auth.Services.JwtService.Implementations;
using Innowise.Clinic.Auth.Services.JwtService.Interfaces;
using Innowise.Clinic.Auth.Services.MailService.Data;
using Innowise.Clinic.Auth.Services.MailService.Implementations;
using Innowise.Clinic.Auth.Services.MailService.Interfaces;
using Innowise.Clinic.Auth.Services.UserCredentialsGenerationService.Implementations;
using Innowise.Clinic.Auth.Services.UserCredentialsGenerationService.Interfaces;
using Innowise.Clinic.Auth.Services.UserManagementService.Implementations;
using Innowise.Clinic.Auth.Services.UserManagementService.Interfaces;
using Innowise.Clinic.Auth.Validators.Custom;
using Innowise.Clinic.Auth.Validators.Identity;
using Innowise.Clinic.Auth.Validators.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Innowise.Clinic.Auth.Configuration;

public static class ConfigurationManager
{
    public static IServiceCollection ConfigureSecurity(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtConfiguration = configuration.GetSection("JWT");
        var jwtValidationConfiguration = configuration.GetSection("JwtValidationConfiguration");

        services.AddAuthentication();
        services.ConfigureIdentity(jwtValidationConfiguration);
        services.ConfigureJwtAuthentication(jwtConfiguration, jwtValidationConfiguration);
        services.ConfigureCustomValidators();
        services.AddSingleton<AuthenticationExceptionHandlingMiddleware>();

        return services;
    }

    public static IServiceCollection ConfigureUserManagementServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailHandler, EmailHandler>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IUserCredentialsGenerationService, UserCredentialsGenerationService>();
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
        var smtpConfiguration = configuration.GetSection("AuthSmtp");
        services.Configure<JwtSettings>(jwtConfiguration);
        services.Configure<SmtpSettings>(smtpConfiguration);
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
            if ((await context.Database.GetPendingMigrationsAsync()).Any()) await context.Database.MigrateAsync();

            var userManager = services.GetRequiredService<UserManager<IdentityUser<Guid>>>();
            await new DataSeeder(userManager).SeedUsers();
        }
    }

    private static IServiceCollection ConfigureIdentity(this IServiceCollection services,
        IConfigurationSection jwtValidationConfiguration)
    {
        services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit =
                    Convert.ToBoolean(jwtValidationConfiguration["RequireDigitsInPassword"]);
                options.Password.RequiredLength = Convert.ToInt32(jwtValidationConfiguration["MinimalPasswordLength"]);
                options.Password.RequireLowercase =
                    Convert.ToBoolean(jwtValidationConfiguration["RequireLowercaseLettersInPassword"]);
                options.Password.RequireNonAlphanumeric =
                    Convert.ToBoolean(jwtValidationConfiguration["RequireNonAlphanumericInPassword"]);
                options.Password.RequireUppercase =
                    Convert.ToBoolean(jwtValidationConfiguration["RequireUppercaseLettersInPassword"]);
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
}