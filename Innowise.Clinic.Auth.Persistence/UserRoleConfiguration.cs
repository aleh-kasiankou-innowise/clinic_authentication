using Innowise.Clinic.Auth.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innowise.Clinic.Auth.Persistence;

public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        builder.HasData(
            new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = UserRoles.Patient,
                NormalizedName = UserRoles.Patient.ToUpper()
            },
            new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = UserRoles.Receptionist,
                NormalizedName = UserRoles.Receptionist.ToUpper()
            },
            new IdentityRole<Guid>()
            {
                Id = Guid.NewGuid(),
                Name = UserRoles.Doctor,
                NormalizedName = UserRoles.Doctor.ToUpper()
            }
        );
    }
}