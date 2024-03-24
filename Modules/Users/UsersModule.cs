using System.Text;
using KidsMealApi.Modules.Users.Adapters;
using KidsMealApi.Modules.Users.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using KidsMealApi.Modules.Users.Ports;
using Microsoft.AspNetCore.Mvc;
using KidsMealApi.Modules.Interfaces;
using System.Collections.Specialized;

namespace KidsMealApi.Modules.Users
{
    /// <summary>
    /// This module contains and exposes all features/functionalities directly 
    /// associated to users.
    /// </summary>
    public class UsersModule : IModule
    {
        private int _refreshTokenExpiration { get; set; }
        
        public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/login", [AllowAnonymous] async (LoginRequest request, [FromServices]UsersModuleFacade facade) => {
                var validationResults = await facade.UserService.ValidateLoginRequestAsync(request);
                if (validationResults.Status == LoginValidationStatus.USER_NOT_FOUND)
                    return Results.BadRequest("Invalid email/password combination. Please try again.");
                
                if (validationResults.Status == LoginValidationStatus.BAD_LOGIN)
                    return Results.BadRequest("Invalid email/password combination. Please try again.");
                
                var existingUser = validationResults.User;
                if (validationResults.Status != LoginValidationStatus.VALID || existingUser == null)
                    throw new Exception("Unable to login at this time. Please try again later.");

                var accessToken = facade.TokenService.BuildAccessToken(existingUser);
                var refreshTokenResults = facade.TokenService.BuildRefreshToken(existingUser);
                if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshTokenResults.RefreshToken))
                    throw new Exception("Unable to login at this time. Please try again later.");

                var wasAuthorizationDetailsUpdated = await facade.UserService.UpdateAuthorizationDetailsAsync(existingUser, refreshTokenResults.RefreshToken, DateTime.UtcNow.AddMinutes(_refreshTokenExpiration), DateTime.UtcNow);
                if (!wasAuthorizationDetailsUpdated)
                    throw new Exception("Unable to complete login at this time. Please try again later.");

                await facade.RefreshTokenHistoryService.ClearAllAsync(existingUser.Id);
                var response = new LoginResponse(existingUser.Id, existingUser.Name, accessToken, refreshTokenResults.RefreshToken);
                response.Kids = existingUser.KidAssociations.Select(ka => new BasicKidInfo { Id = ka.KidId, Name = ka.Kid.FullName, ProfilePicUrl = ka.Kid.ProfilePicUrl }).ToList();
                return Results.Ok(response);

            });

            endpoints.MapGet("/logout/{userID}", [Authorize] async (int userId, [FromServices] UsersModuleFacade facade) => {

                facade.ModuleLogger?.LogInformation("LOGINGGGGGGG OUUUTTTTT");

                var user = await facade.UserService.GetUserByIDAsync(userId);
                if (user == null)
                    return Results.NotFound();

                await facade.UserService.ClearAuthorizationDetailsAsync(user);
                await facade.RefreshTokenHistoryService.ClearAllAsync(userId);
                return Results.Ok("Logged Out");

            });

            endpoints.MapPost("/token/refresh", [AllowAnonymous] async (RefreshTokenRequest request, [FromServices] UsersModuleFacade facade) => {
                
                if (request == null)
                    throw new ArgumentNullException(nameof(request));
                
                var accessToken = request.AccessToken;
                var refreshToken = request.RefreshToken;
                if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
                    return Results.BadRequest("Invalid client request");
                
                var principal = facade.TokenService.GetPrincipalFromAccessToken(accessToken);
                var emailAddress = principal?.Identity?.Name; //user's email address
                if (string.IsNullOrWhiteSpace(emailAddress))
                    return Results.BadRequest("Invalid Token");
                
                var user = facade.UserService.GetUserByEmail(emailAddress);
                if (user == null)
                    return Results.BadRequest("Invalid client request.");

                var tokenFamilyForUser = facade.RefreshTokenHistoryService.GetTokenFamily(user.Id);
                var refreshTokenValidationResults = facade.TokenService.ValidateRefreshToken(refreshToken, user, tokenFamilyForUser);

                if(!refreshTokenValidationResults.IsValid)
                {
                    var errorMessage = "Invalid client request.";
                    if (refreshTokenValidationResults.ValidationErrorCode == RefreshTokenValidationErrors.REUSE_DETECTED)
                    {
                        //User Session was possibly compromised because an old refresh token was used; 
                        //reset refresh token details and just have the user login again for safe measure
                        await facade.UserService.ClearAuthorizationDetailsAsync(user);
                        errorMessage = "Please log in again for security purposes.";
                    }

                    return Results.BadRequest(errorMessage);
                }
                
                //Get new access token
                var newAccessToken = facade.TokenService.BuildAccessToken(user);

                //Rotate refresh token, do not update refresh token expiration EXCEPT for when
                //the refresh token expiration has been reached for a recently issued refresh token
                var refreshTokenResults = facade.TokenService.BuildRefreshToken(user);
                var newRefreshToken = refreshTokenResults.RefreshToken;

                 //Extend refresh token expiration because of recently issued refresh token
                var refreshTokenExpiration = (refreshTokenResults.UpdatedTokenExpiration.HasValue) ? refreshTokenResults.UpdatedTokenExpiration.Value : user.RefreshTokenExpiration;
                await facade.UserService.UpdateAuthorizationDetailsAsync(user, newRefreshToken, refreshTokenExpiration, DateTime.UtcNow);
                
                await facade.RefreshTokenHistoryService.RecycleTokenFamilyAsync(user.Id, refreshToken);

                return Results.Ok(new RefreshTokenResponse(newAccessToken, newRefreshToken));

            });

            return endpoints;
        }

        public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
        {
            var key = configuration["Jwt:Key"] ?? string.Empty;
            var issuer = configuration["Jwt:Issuer"] ?? string.Empty;
            var audience = configuration["Jwt:Audience"] ?? string.Empty;
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                throw new TypeInitializationException( typeof(TokenService).FullName, new("Missing configurations for token service"));

            //Inject TokenService into WebApplicationBuilder services
            services.AddSingleton<ITokenService, TokenService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IRefreshTokenHistoryService, RefreshTokenHistoryService>();
            services.AddTransient<UsersModuleFacade>();

            services.AddAuthorization();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt => {
            
                opt.TokenValidationParameters = new () {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    // set clock skew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };

            });

            services.Configure<UserServiceOptions>(configuration.GetSection(UserServiceOptions.CustomAWSSection));
            services.Configure<TokenServiceOptions>(configuration.GetSection(TokenServiceOptions.JwtSection));

            _refreshTokenExpiration = int.TryParse(configuration["Jwt:RefreshTokenExpiration"], out int parsedSetting) ? parsedSetting : 3600;

            return services;
        }

    }
}