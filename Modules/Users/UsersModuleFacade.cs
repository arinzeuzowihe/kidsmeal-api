using KidsMealApi.Modules.Interfaces;
using KidsMealApi.Modules.Users.Ports;

namespace KidsMealApi.Modules.Users
{
    /// <summary>
    /// Abstraction of all the dependent services for offering
    /// the features/functionalities directly associated to users. 
    /// </summary>
    public class UsersModuleFacade : IModuleFacade<UsersModule>
    {
        public UsersModuleFacade( 
                                    ITokenService tokenService,
                                    IUserService userService,
                                    IRefreshTokenHistoryService refreshTokenHistoryService,
                                    ILogger<UsersModule> logger
                                )
        {
            TokenService = tokenService;
            UserService = userService;
            RefreshTokenHistoryService = refreshTokenHistoryService;
            ModuleLogger = logger;
        }

        public ITokenService TokenService { get; set; }
        public IUserService UserService { get; set; }
        public IRefreshTokenHistoryService RefreshTokenHistoryService { get; set; }
    }
}