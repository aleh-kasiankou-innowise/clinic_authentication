using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innowise.Clinic.Auth.Persistence.Extensions;

public static class DependencyHandler
{
    public static IServiceCollection RegisterDbContext(this IServiceCollection serviceCollection,
        string connectionString)
    {
        serviceCollection.AddDbContext<ClinicAuthDbContext>(
            opt => opt.UseSqlServer(connectionString));
        return serviceCollection;
    }
}