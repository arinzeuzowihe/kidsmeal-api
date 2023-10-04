using System.Diagnostics.CodeAnalysis;

namespace KidsMealApi.DataAccess.Models
{
    public class KidAssociation
    {
        public int Id { get; set; }
        public int KidId { get; set; }
        public int UserId { get; set; }
        [AllowNull]
        public virtual Kid Kid { get; set; }
        [AllowNull]
        public virtual User User { get; set; }
    }
}