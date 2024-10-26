using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace rubberduckvba.com.Server.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static string AsJWT(this ClaimsPrincipal principal, string secret, string issuer, string audience)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var token = new JwtSecurityToken(issuer, audience, 
            claims: principal?.Claims,
            notBefore: new DateTimeOffset(DateTime.UtcNow).UtcDateTime,
            expires: new DateTimeOffset(DateTime.UtcNow.AddMinutes(60)).UtcDateTime,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
