using Microsoft.AspNetCore.Identity;

namespace Innowise.Clinic.Auth.Persistence.Models;

public record RefreshToken
{
    // TODO ADD EXPIRATION TIME TO CLEAR DB IN BACKGROUND WITH HANGFIRE
    // OR Check the issue date and revoke token has expired
    public Guid TokenId { get; init; }
    public Guid UserId { get; init; }
    public virtual IdentityUser<Guid> User { get; init; }
}