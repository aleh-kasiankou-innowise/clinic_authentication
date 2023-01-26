using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Innowise.Clinic.Auth.Persistence;

public class ClinicAuthDbContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
{
    public ClinicAuthDbContext(DbContextOptions<ClinicAuthDbContext> options ) : base(options)
    {
        
    }
}