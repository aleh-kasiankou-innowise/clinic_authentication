using System.Reflection;
using Innowise.Clinic.Auth.DependencyConfiguration;
using Innowise.Clinic.Auth.Dto;
using Innowise.Clinic.Auth.Jwt;
using Innowise.Clinic.Auth.Mail;
using Innowise.Clinic.Auth.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
        $"{Assembly.GetAssembly(typeof(PatientCredentialsDto))?.GetName().Name}.xml"));
});
builder.Services.AddDbContext<ClinicAuthDbContext>(
    opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("default")));
builder.Services.AddAuthentication();
builder.Services.ConfigureCustomValidators();
builder.Services.ConfigureIdentity();
builder.Services.Configure<JwtData>(builder.Configuration.GetSection("JWT"));
builder.Services.Configure<SmtpData>(builder.Configuration.GetSection("AuthSmtp"));
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.ConfigureJwtAuthentication(builder.Configuration.GetSection("JWT"));
builder.Services.ConfigureUserManagementServices();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ClinicAuthDbContext>();
    if (context.Database.GetPendingMigrations().Any()) context.Database.Migrate();
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