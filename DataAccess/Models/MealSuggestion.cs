using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace KidsMealApi.DataAccess.Models
{
    public class MealSuggestion
    {
        [Key]
        public int Id { get; set; }
        public int KidId { get; init; }
        [AllowNull]
        public string MealName { get; init; }
        [AllowNull]
        public string MealDescription { get; init; }
        public MealType MealType { get; init; }
        public bool IsConfirmed { get; set; }
        public DateTime CreatedOn { get; init; }

        public MealSuggestion()
        {
            CreatedOn = DateTime.UtcNow;
        }

        public MealSuggestion(int kidId, Meal meal, MealType mealType)
        {
            KidId = kidId;
            MealName = meal.Name;
            MealDescription = meal.Description;
            MealType = mealType;
            CreatedOn = DateTime.UtcNow;
        }
    }

    public enum MealType
    {
        Breakfast,
        Lunch,
        Snack,
        Dinner
    }
}