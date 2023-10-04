using System.Security.Claims;
using KidsMealApi.DataAccess.Models;

namespace KidsMealApi.Modules.Users.Ports
{
    public interface ITokenService
    {
        string BuildAccessToken(User user);
        (string RefreshToken, DateTime? UpdatedTokenExpiration) BuildRefreshToken(User user);
        ClaimsPrincipal GetPrincipalFromAccessToken(string accessToken);
        (bool IsValid, RefreshTokenValidationErrors? ValidationErrorCode) ValidateRefreshToken(string refreshToken, User user, IEnumerable<RefreshTokenHistory> tokenFamilyForUser);
    }

    public enum RefreshTokenValidationErrors
    {
        REUSE_DETECTED, //A recently revoked token was used 
        USER_MISMATCH, //Token does not match the active token associated to the user
        EXPIRED //Token has expired and issue date did not meet eligiblity for auto-refresh

    }
}