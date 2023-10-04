using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using KidsMealApi.DataAccess.Interfaces;

namespace KidsMealApi.DataAccess.Models
{
    public class Meal : IUniqueEntity
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsSideDish { get; set; }
        public bool IsTakeout { get; set; }

        /*[AllowNull]
        public virtual ICollection<MealPreference> MealPreferences { get; set; }*/
    }
}