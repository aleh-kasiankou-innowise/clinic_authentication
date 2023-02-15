using System.Reflection;
using Innowise.Clinic.Auth.Configuration;
using Innowise.Clinic.Auth.Configuration.Swagger;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Middleware;
using Innowise.Clinic.Auth.Persistence;
using Innowise.Clinic.Auth.Services.JwtService.Data;
using Innowise.Clinic.Auth.Services.MailService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

//TODO MOVE THE CODE TO A DIFFERENT PLACE

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.ConfigureSwaggerJwtSupport();
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetAssembly(typeof(AuthTokenPairDto))?.GetName().Name}.xml"));
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetAssembly(typeof(UserCredentialsDto))?.GetName().Name}.xml"));
    options.ExampleFilters();
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<SignInCredentialsExample>();

builder.Services.AddDbContext<ClinicAuthDbContext>(
    opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("default")));
builder.Services.AddAuthentication();
builder.Services.ConfigureCustomValidators();
builder.Services.ConfigureIdentity(builder.Configuration.GetSection("JwtValidationConfiguration"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("AuthSmtp"));
builder.Services.Configure<JwtValidationSettings>(builder.Configuration.GetSection("JwtValidationConfiguration"));
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.ConfigureJwtAuthentication(builder.Configuration.GetSection("JWT"),
    builder.Configuration.GetSection("JWT"));
builder.Services.ConfigureUserManagementServices();
builder.Services.AddSingleton<AuthenticationExceptionHandlingMiddleware>();

var app = builder.Build();

app.UseMiddleware<AuthenticationExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ClinicAuthDbContext>();
    if (context.Database.GetPendingMigrations().Any()) context.Database.Migrate();

    var userManager = services.GetRequiredService<UserManager<IdentityUser<Guid>>>();
    await new DataSeeder(userManager).SeedUsers();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

namespace Innowise.Clinic.Auth.Api
{
    public class Program
    {
    }
}