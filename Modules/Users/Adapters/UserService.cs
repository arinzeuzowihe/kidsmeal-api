using System.Reflection.Metadata;
using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using KidsMealApi.DataAccess;
using KidsMealApi.DataAccess.Models;
using KidsMealApi.Modules.Users.Core;
using KidsMealApi.Modules.Users.Ports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KidsMealApi.Modules.Users.Adapters
{
    public class UserService : BaseDataAccessService<User>, IUserService
    {
        private const string s3ObjectAttribute = "ObjectSize";
        private const string s3ProfileObjectPrefix = "profile";
        private const string s3ProfileObjectFileExtension = ".png";
        private readonly UserServiceOptions _options;

        public UserService(KidsMealDbContext dbContext, 
                           IOptions<UserServiceOptions> options):base(dbContext)
        {
            _options = options.Value;
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

        public async Task<(LoginValidationStatus, User?)> ValidateLoginRequestAsync(LoginRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            
            var username = request.Username;
            var password = request.Password;
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));
            
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            var existingUser = await lookUpUserByEmailAsync(username, true);
            if (existingUser == null)
                return (LoginValidationStatus.USER_NOT_FOUND, null);

            //https://jasonwatmore.com/post/2020/07/16/aspnet-core-3-hash-and-verify-passwords-with-bcrypt
            if (!BCrypt.Net.BCrypt.Verify(password, existingUser.Password))
                return (LoginValidationStatus.BAD_LOGIN, null);

            return (LoginValidationStatus.VALID, existingUser);
        }

        private async Task<User?> lookUpUserByEmailAsync(string emailAddress, bool includeChildInfo = false)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                throw new ArgumentNullException(nameof(emailAddress));

            var query = GetAll();
            if (includeChildInfo)
            {
                query = query.Include(u => u.KidAssociations)
                             .ThenInclude(ka => ka.Kid);
            }
            
            var foundUser = query.FirstOrDefault(u => u.Email == emailAddress);
            if (foundUser == null)
            {
                return null;
            }
            var kids = foundUser.KidAssociations.Select(ka => ka.Kid).ToList();
            if (kids.Any())
            {
                await populateKidProfilePicUrlsAsync(kids);
            }

            return foundUser;
        }

        private async Task populateKidProfilePicUrlsAsync(List<Kid> kids)
        {
            var chain = new CredentialProfileStoreChain();
            AWSCredentials aWSCredentials;
            var profilePicDomain = new StringBuilder();
            if (chain.TryGetAWSCredentials(_options.ProfileName, out aWSCredentials))
            {
                var regionEndPoint = RegionEndpoint.GetBySystemName(_options.Region);
                using (var client = new AmazonS3Client(aWSCredentials, regionEndPoint))
                {
                    var bucketResponse = await client.ListBucketsAsync();
                    var bucket = bucketResponse.Buckets.FirstOrDefault(b => b.BucketName == _options.S3BucketName);
                    if (bucket == null)
                    {
                        return;
                    }

                    profilePicDomain.Append($"{bucket.BucketName}");
                    profilePicDomain.Append($".{client.Config.AuthenticationServiceName}");
                    profilePicDomain.Append($".{client.Config.RegionEndpoint.SystemName}");
                    profilePicDomain.Append($".{client.Config.RegionEndpoint.PartitionDnsSuffix}");

                    var objectSizeAttribute = new ObjectAttributes(s3ObjectAttribute);
                    foreach (var kid in kids)
                    {
                        var profilePicFileName = s3ProfileObjectPrefix + kid.Id.ToString() + s3ProfileObjectFileExtension;
                        // check that the objects exists other display default pic
                        var attributesRequest = new GetObjectAttributesRequest
                        {
                            BucketName = bucket.BucketName,
                            Key = profilePicFileName,
                            ObjectAttributes = new List<ObjectAttributes>{objectSizeAttribute}
                        };

                        var attributeResponse = await client.GetObjectAttributesAsync(attributesRequest);
                        if (attributeResponse?.ObjectSize > 0)
                        {
                            var objectUrl = new UriBuilder("https", profilePicDomain.ToString());
                            objectUrl.Path = profilePicFileName;
                            kid.ProfilePicUrl = objectUrl.ToString();
                        }
                    }
                }
            }
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