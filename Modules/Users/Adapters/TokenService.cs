using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KidsMealApi.Modules.Users.Ports;
using Microsoft.IdentityModel.Tokens;
using KidsMealApi.DataAccess.Models;

namespace KidsMealApi.Modules.Users.Adapters
{
    public class TokenService : ITokenService
    {
        public string Key { get; init; }
        public string Issuer { get; init; }
        public string Audience { get; init; }
        public int TokenExpirationPeriod { get; init; }
        public int IssuanceThreshold { get; set; }

        public TokenService (string key, string issuer, string audience)
        {
            Key = key;
            Issuer = issuer;
            Audience = audience;
        } 

        public string BuildAccessToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (user.Name == null)
                throw new Exception($"Invalid username while calling {nameof(BuildAccessToken)}");
            
            var claims = new [] {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new JwtSecurityToken(Issuer, Audience, claims, expires: DateTime.Now.AddHours(1), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public (string RefreshToken, DateTime? UpdatedTokenExpiration) BuildRefreshToken(User user)
        {
            
            var randomNumber = new byte[32];
            var refreshToken = string.Empty;
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                refreshToken = Convert.ToBase64String(randomNumber);
            }

            DateTime? updatedExpiration = null;
            if (isExpirationEligibleForExtension(user.RefreshTokenIssuance, user.RefreshTokenExpiration))
                updatedExpiration = user.RefreshTokenExpiration.AddDays(1);
            
            return (refreshToken, updatedExpiration);
        }

        public ClaimsPrincipal GetPrincipalFromAccessToken(string accessToken)
        {
            var tokenValidationParameters = new TokenValidationParameters {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = Audience,
                ValidIssuer = Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                ValidateLifetime = false //TODO: re-evaluate if I want to turn this on
            };

            var tokenhandler = new JwtSecurityTokenHandler();
            var principal = tokenhandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken validatedToken);
            var jwtSecurityToken = validatedToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid Token");
            
            return principal;
        }

        public (bool IsValid, RefreshTokenValidationErrors? ValidationErrorCode) ValidateRefreshToken(string refreshToken, User user, IEnumerable<RefreshTokenHistory> tokenFamilyForUser)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentNullException(nameof(refreshToken));
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (tokenFamilyForUser == null)
                throw new ArgumentNullException(nameof(tokenFamilyForUser));
            
            //Validate that the refresh token is not an old/inactive token in the token family
            if (tokenFamilyForUser.Any(t => t.RefreshToken == refreshToken))
                return (false, RefreshTokenValidationErrors.REUSE_DETECTED);

            //Validate that refresh token is for an existing user
            if (user.RefreshToken != refreshToken)
                return (false, RefreshTokenValidationErrors.USER_MISMATCH);
                    
            //Check if refresh token's expiration has been reached without a refresh token being issued recently
            var wasRefreshTokenRecentlyIssued = user.RefreshTokenIssuance.AddHours(1) > DateTime.Now;
            var hasRefreshTokenExpired = user.RefreshTokenExpiration <= DateTime.Now;
            if (hasRefreshTokenExpired && !wasRefreshTokenRecentlyIssued)
                return (false, RefreshTokenValidationErrors.EXPIRED);
            
            return (true, null);
        }

        private bool isExpirationEligibleForExtension(DateTime issueDateTime, DateTime expirationDateTime)
        {
            var wasRefreshTokenRecentlyIssued = issueDateTime.AddHours(1) > DateTime.Now;
            var hasRefreshTokenExpired = expirationDateTime <= DateTime.Now;            
            return hasRefreshTokenExpired && wasRefreshTokenRecentlyIssued;
        }
    }
}