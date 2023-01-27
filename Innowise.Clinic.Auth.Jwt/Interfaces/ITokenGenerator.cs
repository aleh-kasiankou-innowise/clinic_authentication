using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Innowise.Clinic.Auth.Jwt.Interfaces;

public interface ITokenGenerator
{
    string GenerateToken(IEnumerable<Claim> authClaims);
}