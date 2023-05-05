using Innowise.Clinic.Auth.Configuration;
using Innowise.Clinic.Auth.Middleware;
using Innowise.Clinic.Auth.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger();
builder.Services.AddDbContext<ClinicAuthDbContext>(
    opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("default")));
builder.Services.ConfigureSecurity(builder.Configuration);
builder.Services.AddConfigurationOptions(builder.Configuration);
builder.Services.ConfigureUserManagementServices();
builder.Services.ConfigureCrossServiceCommunication(builder.Configuration);
builder.ConfigureSerilog();

var app = builder.Build();

await app.PrepareDatabase();

app.UseMiddleware<AuthenticationExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Log.Information("The Authentication service is starting");
app.Run();
Log.Information("The Authentication service is stopping");
await Log.CloseAndFlushAsync();

namespace Innowise.Clinic.Auth.Api
{
    public class Program
    {
    }
}