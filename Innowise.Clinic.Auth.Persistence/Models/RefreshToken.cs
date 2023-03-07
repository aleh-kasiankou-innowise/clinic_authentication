using Microsoft.AspNetCore.Identity;

namespace Innowise.Clinic.Auth.Persistence.Models;

public record RefreshToken
{
    public Guid TokenId { get; init; }
    public Guid UserId { get; init; }
    public virtual IdentityUser<Guid> User { get; init; }
}