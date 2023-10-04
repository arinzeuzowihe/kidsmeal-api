using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using KidsMealApi.DataAccess.Interfaces;

namespace KidsMealApi.DataAccess.Models
{
    public class MealPreference : IUniqueEntity
    {
        public MealPreference(int mealId, int kidId, MealType mealType, bool isActive)
        {
            MealId = mealId;
            KidId = kidId;
            MealType = mealType;
            IsActive = isActive;
        }
        
        [Key]
        public int Id { get; set; }   
        public int MealId { get; init; }
        public int KidId { get; init; }
        public MealType MealType {get; init; }
        public bool IsActive { get; set; }

        #region Navigation Properties
        [AllowNull]
        public virtual Meal Meal { get; set; }
        #endregion

    }
}