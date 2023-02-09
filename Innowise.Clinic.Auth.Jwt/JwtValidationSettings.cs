namespace Innowise.Clinic.Auth.Jwt;

public class JwtValidationSettings
{
    public bool ValidateIssuer { get; init; }
    public bool ValidateIssuerSigningKey { get; init; }
    public bool ValidateAudience { get; init; }
    public bool ValidateLifetime { get; init; }
}