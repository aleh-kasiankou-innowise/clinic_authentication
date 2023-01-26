using Microsoft.EntityFrameworkCore;

namespace Innowise.Clinic.Auth.Persistence;

public class ClinicAuthDbContext : DbContext
{
    public ClinicAuthDbContext(DbContextOptions<ClinicAuthDbContext> options ) : base(options)
    {
        
    }
}