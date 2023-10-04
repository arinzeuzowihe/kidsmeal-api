using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using KidsMealApi.DataAccess.Interfaces;

namespace KidsMealApi.DataAccess.Models
{
    public record User : IUniqueEntity
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiration { get; set; }
        public DateTime RefreshTokenIssuance { get; set; }
        public DateTime CreatedOn { get; set; }
        public User()
        {
            
        }
        [AllowNull]
        public virtual ICollection<KidAssociation> KidAssociations { get; set; }
    }
}