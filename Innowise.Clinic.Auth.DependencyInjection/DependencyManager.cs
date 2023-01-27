using System.Text;
using Innowise.Clinic.Auth.Jwt;
using Innowise.Clinic.Auth.Jwt.Interfaces;
using Innowise.Clinic.Auth.Persistence;
using Innowise.Clinic.Auth.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Innowise.Clinic.Auth.DependencyInjection;

public static class DependencyManager
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
            .AddPasswordValidator<MaximalPasswordLengthValidator<IdentityUser<Guid>>>();

        return services;
    }

    public static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services,
        IConfigurationSection jwtData)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = jwtData["ValidIssuer"],
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtData["Key"]))
            };
        });

        services.AddSingleton<ITokenGenerator, TokenGenerator>();

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
}