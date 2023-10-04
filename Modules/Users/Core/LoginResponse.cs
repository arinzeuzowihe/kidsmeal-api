using System.Diagnostics.CodeAnalysis;

namespace KidsMealApi.Modules.Users.Core
{
    public class LoginResponse
    {
        public LoginResponse(int userID, string userName, string accessToken, string refreshToken)
        {
            UserID = userID;
            UserName = userName;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            Kids = new List<BasicKidInfo>();
        }

        public int UserID { get; set; }

        public string UserName { get; init; } = string.Empty;

        public string AccessToken { get; init; } = string.Empty;

        public string RefreshToken { get; init; } = string.Empty;

        [AllowNull]
        public List<BasicKidInfo> Kids{ get; set; }
    }

    public class BasicKidInfo
    {
        public int Id { get; set; }
        [AllowNull]
        public string Name { get; set; }
    }
}