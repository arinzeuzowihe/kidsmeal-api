using System.ComponentModel.DataAnnotations;

namespace KidsMealApi.DataAccess.Models
{
    public class RefreshTokenHistory
    {
        public RefreshTokenHistory()
        {
            
        }

        public RefreshTokenHistory(int userId, string refreshToken)
        {
            UserId = userId;
            RefreshToken = refreshToken;
            RevokedOn = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RevokedOn { get; set; }
    }
}