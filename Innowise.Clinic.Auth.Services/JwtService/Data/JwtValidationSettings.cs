namespace Innowise.Clinic.Auth.Services.JwtService.Data;

public class JwtValidationSettings
{
    // TODO Separate to different classes: JWT - Password - Email
    public bool ValidateIssuer { get; init; }
    public bool ValidateIssuerSigningKey { get; init; }
    public bool ValidateAudience { get; init; }
    public bool ValidateLifetime { get; init; }
    public bool RequireDigitsInPassword { get; set; } = true;
    public int MinimalPasswordLength { get; set; } = 6;
    public int MaximalPasswordLength { get; set; } = 15;
    public bool RequireLowercaseLettersInPassword { get; set; } = true;
    public bool RequireNonAlphanumericInPassword { get; set; } = true;
    public bool RequireUppercaseLettersInPassword { get; set; } = true;
    public bool ValidateUserEmailConfirmedOnLogin { get; set; } = true;
}