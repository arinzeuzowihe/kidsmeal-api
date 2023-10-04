using KidsMealApi.DataAccess;
using KidsMealApi.DataAccess.Models;
using KidsMealApi.Modules.Users.Core;

namespace KidsMealApi.Modules.Users.Ports
{
    public interface IUserService
    {
        Task<User> GetUserByIDAsync(int userId);
        User GetUserByEmail(string emailAddress);
        (LoginValidationStatus Status, User? User) ValidateLoginRequest(LoginRequest request);
        Task<bool> UpdateAuthorizationDetailsAsync(User user, string refreshToken, DateTime refreshTokenExpiration, DateTime refreshTokenIssuance);
        Task<bool> ClearAuthorizationDetailsAsync(User user);
    }

    public enum LoginValidationStatus
    {
        VALID,
        USER_NOT_FOUND,
        BAD_LOGIN
    }
}