using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Innowise.Clinic.Auth;

public class TokenGenerator : ITokenGenerator
{
    private readonly IOptions<JwtData> _jwtOptions;
    
    public TokenGenerator(IOptions<JwtData> jwtOptions)
    {
        _jwtOptions = jwtOptions;
    }
    
    public JwtSecurityToken GenerateToken(IEnumerable<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Value.Key));

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Value.ValidIssuer,
            expires: DateTime.Now.AddHours(1),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }
}