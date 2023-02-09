using System.Text;
using Innowise.Clinic.Auth.Jwt;
using Innowise.Clinic.Auth.Jwt.Interfaces;
using Innowise.Clinic.Auth.Mail;
using Innowise.Clinic.Auth.Mail.Interfaces;
using Innowise.Clinic.Auth.Persistence;
using Innowise.Clinic.Auth.UserManagement;
using Innowise.Clinic.Auth.UserManagement.interfaces;
using Innowise.Clinic.Auth.Validators.Custom;
using Innowise.Clinic.Auth.Validators.Identity;
using Innowise.Clinic.Auth.Validators.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Innowise.Clinic.Auth.DependencyConfiguration;

public static class ConfigurationManager
{
    public static IServiceCollection ConfigureIdentity(this IServiceCollection services)
    {
        services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
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

    public static IServiceCollection ConfigureUserManagementServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailHandler, EmailHandler>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        return services;
    }

    public static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services,
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

        return services;
    }

    public static void ConfigureSwaggerJwtSupport(this SwaggerGenOptions options)
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
    }

    public static IServiceCollection ConfigureCustomValidators(this IServiceCollection services)
    {
        services.AddScoped<ITokenValidator, TokenValidator>();
        services.AddScoped<IEmailValidator, EmailUniquenessValidator>();

        return services;
    }
}