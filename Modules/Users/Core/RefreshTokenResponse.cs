namespace KidsMealApi.Modules.Users.Core
{
    public class RefreshTokenResponse
    {
        public RefreshTokenResponse(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public string AccessToken { get; init; } = string.Empty;

        public string RefreshToken { get; init; } = string.Empty;
    }
}