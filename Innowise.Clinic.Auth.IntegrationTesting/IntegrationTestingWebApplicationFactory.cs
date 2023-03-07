using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Innowise.Clinic.Auth.Api;
using Innowise.Clinic.Auth.Persistence;
using Innowise.Clinic.Auth.Services.JwtService.Data;
using Innowise.Clinic.Auth.Services.MailService.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innowise.Clinic.Auth.IntegrationTesting;

public class IntegrationTestingWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ContainerHost = "localhost";
    private const string ContainerDbName = "AuthDb";
    private const string ContainerDbUserName = "SA";
    private const string ContainerDbPassword = "secureMssqlServerPassw0rd";

    private readonly TestcontainersContainer _dbContainer;
    private readonly int _dbPort = GetFreeTcpPort();
    private readonly int _imapPort = GetFreeTcpPort();
    private readonly TestcontainersContainer _mailContainer;
    private readonly int _smtpPort = GetFreeTcpPort();

    public IntegrationTestingWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("GATEWAY_URL", "No gateway for testing");
        _dbContainer = PrepareDbContainer();
        _mailContainer = PrepareMailContainer();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _mailContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _mailContainer.StopAsync();
        await base.DisposeAsync();
    }

    public T UseDbContext<T>(Func<ClinicAuthDbContext, T> func)
    {
        using (var scope = Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ClinicAuthDbContext>();
            return func(db);
        }
    }

    public T UseConfiguration<T>(Func<IConfiguration, T> func)
    {
        using (var scope = Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var config = scopedServices.GetRequiredService<IConfiguration>();
            return func(config);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<ClinicAuthDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ClinicAuthDbContext>(options =>
            {
                options.UseSqlServer(BuildConnectionString(_dbPort));
            });

            services.Configure<JwtSettings>(x => { x.TokenValidityInSeconds = 5; });
            services.Configure<SmtpSettings>(x =>
            {
                x.SmtpServerHost = ContainerHost;
                x.SmtpServerPort = _smtpPort;
            });
        });
    }

    private TestcontainersContainer PrepareDbContainer()
    {
        return new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithEnvironment("SA_PASSWORD", "secureMssqlServerPassw0rd").WithEnvironment(
                "ACCEPT_EULA", "Y").WithPortBinding(_dbPort, 1433)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433)).Build();
    }

    private TestcontainersContainer PrepareMailContainer()
    {
        return new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("rnwood/smtp4dev:v3")
            .WithPortBinding(_smtpPort, 25)
            .WithPortBinding(_imapPort, 143)
            .WithEnvironment("ServerOptions__HostName", "smtp4dev")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(25)).Build();
    }

    private static string BuildConnectionString(int port)
    {
        return
            $"Server={ContainerHost},{port};Database={ContainerDbName};User Id={ContainerDbUserName};Password={ContainerDbPassword};";
    }

    private static int GetFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}