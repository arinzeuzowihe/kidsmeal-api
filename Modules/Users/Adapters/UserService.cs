using KidsMealApi.DataAccess;
using KidsMealApi.DataAccess.Models;
using KidsMealApi.Modules.Users.Core;
using KidsMealApi.Modules.Users.Ports;
using Microsoft.EntityFrameworkCore;

namespace KidsMealApi.Modules.Users.Adapters
{
    public class UserService : BaseDataAccessService<User>, IUserService
    {
        public UserService(KidsMealDbContext dbContext):base(dbContext)
        {
            
        }

        protected override IQueryable<User> GetAll()
        {
            return _dbContext.Users;
        }

        public async Task<User> GetUserByIDAsync(int userId)
        {
            return await GetByIdAsync(userId);
        }

        public User GetUserByEmail(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                throw new ArgumentNullException(nameof(emailAddress));
            
            var existingUser = GetAll().FirstOrDefault(u => u.Email == emailAddress);
            if (existingUser == null)
                throw new Exception($"User with email '{emailAddress}' does not exist.");

            return existingUser;
        }

        public (LoginValidationStatus, User?) ValidateLoginRequest(LoginRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            
            var username = request.Username;
            var password = request.Password;
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));
            
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            var existingUser = lookUpUserByEmail(username, true);
            if (existingUser == null)
                return (LoginValidationStatus.USER_NOT_FOUND, null);

            //https://jasonwatmore.com/post/2020/07/16/aspnet-core-3-hash-and-verify-passwords-with-bcrypt
            if (!BCrypt.Net.BCrypt.Verify(password, existingUser.Password))
                return (LoginValidationStatus.BAD_LOGIN, null);

            return (LoginValidationStatus.VALID, existingUser);
        }

        private User? lookUpUserByEmail(string emailAddress, bool includeChildInfo = false)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                throw new ArgumentNullException(nameof(emailAddress));

            var query = GetAll();
            if (includeChildInfo)
            {
                query = query.Include(u => u.KidAssociations)
                             .ThenInclude(ka => ka.Kid);
            }
            
            return query.FirstOrDefault(u => u.Email == emailAddress);
        }

        public async Task<bool> UpdateAuthorizationDetailsAsync(User user, string refreshToken,  DateTime refreshTokenExpiration, DateTime refreshTokenIssuance)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentNullException(nameof(refreshToken));

            try
            {
                await UpdateAsync(user, (userToUpdate) => { 
                    userToUpdate.RefreshToken = refreshToken;
                    userToUpdate.RefreshTokenExpiration = refreshTokenExpiration;
                    userToUpdate.RefreshTokenIssuance = refreshTokenIssuance;
                });
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }

        }

        public async Task<bool> ClearAuthorizationDetailsAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
            try
            {
                await UpdateAsync(user, (userToUpdate) => { 
                    userToUpdate.RefreshToken = string.Empty;
                    userToUpdate.RefreshTokenExpiration = DateTime.UtcNow;
                    userToUpdate.RefreshTokenIssuance = DateTime.MinValue.ToUniversalTime();
                });
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}