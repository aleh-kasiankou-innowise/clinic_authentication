using System.Text;
using Innowise.Clinic.Auth;
using Innowise.Clinic.Auth.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ClinicAuthDbContext>(
    opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("default")));

builder.Services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ClinicAuthDbContext>()
    .AddDefaultTokenProviders();



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration.GetValue<string>("JWT/ValidIssuer"),
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JWT/Key")))
    };
});

builder.Services.Configure<JwtData>(builder.Configuration.GetSection("JWT"));

builder.Services.AddSingleton<ITokenGenerator, TokenGenerator>();

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
}

/*app.UseHttpsRedirection();*/

app.UseAuthorization();

app.MapControllers();

app.Run();