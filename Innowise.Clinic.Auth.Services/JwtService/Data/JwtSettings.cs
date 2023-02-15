namespace Innowise.Clinic.Auth.Services.JwtService.Data;

public record JwtSettings
{
    public string Key { get; init; }
    public string ValidIssuer { get; init; }

    public string ValidAudience { get; init; }


    public int TokenValidityInSeconds { get; set; }

    public int RefreshTokenValidityInDays { get; set; }
}