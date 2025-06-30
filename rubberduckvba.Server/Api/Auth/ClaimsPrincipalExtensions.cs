using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace rubberduckvba.Server.Api.Auth;

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

    /// <summary>
    /// <c>true</c> if the user is authenticated and has the <c>rd-admin</c> role.
    /// </summary>
    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(RDConstants.Roles.AdminRole);
    }

    /// <summary>
    /// <c>true</c> if the user is authenticated and has the <c>rd-reviewer</c> or <c>rd-admin</c> role.
    /// </summary>
    public static bool IsReviewer(this ClaimsPrincipal principal)
    {
        return principal.IsInRole(RDConstants.Roles.ReviewerRole);
    }
}

public static class ClaimsIdentityExtensions
{
    /// <summary>
    /// <c>true</c> if the user is authenticated and has the <c>rd-admin</c> role.
    /// </summary>
    public static bool IsAdmin(this IIdentity identity)
    {
        return identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsAdmin();
    }

    /// <summary>
    /// <c>true</c> if the user is authenticated and has the <c>rd-reviewer</c> or <c>rd-admin</c> role.
    /// </summary>
    public static bool IsReviewer(this IIdentity identity)
    {
        return identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsReviewer();
    }

    /// <summary>
    /// <c>true</c> if the user is authenticated and has the <c>rd-admin</c> role.
    /// </summary>
    public static bool IsAdmin(this ClaimsIdentity identity)
    {
        return identity.IsAuthenticated && identity.HasClaim(ClaimTypes.Role, RDConstants.Roles.AdminRole);
    }
    /// <summary>
    /// <c>true</c> if the user is authenticated and has the <c>rd-reviewer</c> or <c>rd-admin</c> role.
    /// </summary>
    public static bool IsReviewer(this ClaimsIdentity identity)
    {
        return identity != null && identity.IsAuthenticated && (identity.HasClaim(ClaimTypes.Role, RDConstants.Roles.AdminRole) || identity.HasClaim(ClaimTypes.Role, RDConstants.Roles.ReviewerRole));
    }
}