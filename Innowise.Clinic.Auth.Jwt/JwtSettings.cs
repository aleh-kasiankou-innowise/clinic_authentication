namespace Innowise.Clinic.Auth.Jwt;

public record JwtSettings
{
    public string Key { get; init; }
    public string ValidIssuer { get; init; }

    public string ValidAudience { get; init; }


    public int TokenValidityInSeconds { get; set; }

    public int RefreshTokenValidityInDays { get; set; }
}