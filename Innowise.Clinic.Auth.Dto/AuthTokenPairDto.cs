namespace Innowise.Clinic.Auth.Dto;

public class AuthTokenPairDto
{
    public AuthTokenPairDto(string jwtToken, string refreshToken)
    {
        JwtToken = jwtToken;
        RefreshToken = refreshToken;
    }

    private AuthTokenPairDto()
    {
    }

    public string JwtToken { get; set; }
    public string RefreshToken { get; set; }
}