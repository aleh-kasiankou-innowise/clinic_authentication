namespace Innowise.Clinic.Auth.Jwt;

public class JwtValidationSettings
{
    public bool ValidateIssuer { get; init; }
    public bool ValidateIssuerSigningKey { get; init; }
    public bool ValidateAudience { get; init; }
    public bool ValidateLifetime { get; init; }
    public bool RequireDigitsInPassword { get; set; } = true;
    public int MinimalPasswordLength { get; set; } = 6;
    public bool RequireLowercaseLettersInPassword { get; set; } = true;
    public bool RequireNonAlphanumericInPassword { get; set; } = true;
    public bool RequireUppercaseLettersInPassword { get; set; } = true;
}