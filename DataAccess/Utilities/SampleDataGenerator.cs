using KidsMealApi.DataAccess.Models;

namespace KidsMealApi.DataAccess.Utilities
{
    public static class SampleDataGenerator
    {
        public static void SeedDatabase(KidsMealDbContext databaseContext)
        {
            if (databaseContext == null)
                return;
            
            if (!databaseContext.Users.Any())
            {
                databaseContext.Users.Add(new User { 
                                            Name = "deholeskool", 
                                            Password= BCrypt.Net.BCrypt.HashPassword("abc123"), 
                                            RefreshToken = "", 
                                            RefreshTokenExpiration = DateTime.UtcNow, 
                                            RefreshTokenIssuance = DateTime.MinValue.ToUniversalTime(),
                                            CreatedOn = DateTime.UtcNow 
                                        });
            
                databaseContext.SaveChanges();
            }
        }
    }
}