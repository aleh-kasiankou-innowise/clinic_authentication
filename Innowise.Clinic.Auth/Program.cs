using Innowise.Clinic.Auth.Constants;
using Innowise.Clinic.Auth.DependencyInjection;
using Innowise.Clinic.Auth.Jwt;
using Innowise.Clinic.Auth.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.ConfigureSwaggerJwtSupport());
builder.Services.AddDbContext<ClinicAuthDbContext>(
    opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("default")));
builder.Services.AddAuthentication();

builder.Services.ConfigureIdentity();
builder.Services.Configure<JwtData>(builder.Configuration.GetSection("JWT"));
builder.Services.ConfigureJwtAuthentication(builder.Configuration.GetSection("JWT"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ClinicAuthDbContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
        
    }
    
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    var roles = Enum.GetValues(typeof(UserRoles));
    
    foreach (var role in roles)
    {
        var roleName = role.ToString();
        
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(roleName)); //WhenAll ??
        }
    }
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();