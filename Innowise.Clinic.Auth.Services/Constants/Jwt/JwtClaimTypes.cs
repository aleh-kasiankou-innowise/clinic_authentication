namespace Innowise.Clinic.Auth.Services.Constants.Jwt;

public static class JwtClaimTypes
{
    public static readonly string UserIdClaim = "user-id";
    public static readonly string TokenIdClaim = "jti";
    public static readonly string CanInteractWithProfileClaim = "access-to-profiles";
}