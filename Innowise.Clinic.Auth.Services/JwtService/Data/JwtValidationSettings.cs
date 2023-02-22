namespace Innowise.Clinic.Auth.Services.JwtService.Data;

public class JwtValidationSettings
{
    public bool ValidateIssuer { get; init; }
    public bool ValidateIssuerSigningKey { get; init; }
    public bool ValidateAudience { get; init; }
    public bool ValidateLifetime { get; init; }
    public bool RequireDigitsInPassword { get; set; } = true;
}