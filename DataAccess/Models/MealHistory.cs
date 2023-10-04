using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace KidsMealApi.DataAccess.Models
{
    public class MealHistory 
    {
        [Key]
        public int Id { get; set; }
        public int KidId { get; set; }
        public int MealSuggestionID { get; set; }
        public bool WasSuggestionSelected { get; set; }
        [AllowNull]
        public int? AlternateMealID { get; set; }
        [AllowNull]
        public string? AlternateMealName { get; set; }
        [AllowNull]
        public string? AlternateMealDescription { get; set; }
        public int ConfirmedBy  { get; set; }
        public DateTime ConfirmedOn  { get; set; }
        [AllowNull]
        public virtual MealSuggestion MealSuggestion { get; set; }

        public MealHistory()
        {
            
        }

        /// <summary>
        /// Construct meal history object that supports alternate meals that are meal preferences which already
        /// exist in the system. 
        /// </summary>
        /// <param name="mealSuggestion"></param>
        /// <param name="confirmedBy"></param>
        /// <param name="existingAlternateMeal"></param>
        public MealHistory(MealSuggestion mealSuggestion, int confirmedBy, Meal? existingAlternateMeal)
        {
            MealSuggestionID = mealSuggestion.Id;
            KidId = mealSuggestion.KidId;
            ConfirmedBy = confirmedBy;
            ConfirmedOn = DateTime.UtcNow;
            WasSuggestionSelected = true;
            MealSuggestion = mealSuggestion;

            if (existingAlternateMeal != null)
            {
                AlternateMealID = existingAlternateMeal.Id;
                AlternateMealName = existingAlternateMeal.Name;
                AlternateMealDescription = existingAlternateMeal.Description;
                WasSuggestionSelected = false;
            }
        }

        /// <summary>
        /// Construct meal history object that supports alternate meals that were manually entered by
        /// the user.
        /// </summary>
        /// <param name="mealSuggestion"></param>
        /// <param name="confirmedBy"></param>
        /// <param name="alternateMealName"></param>
        /// <param name="alternateMealDescription"></param>
        public MealHistory(MealSuggestion mealSuggestion, int confirmedBy, string? alternateMealName = null, string? alternateMealDescription = null)
        {
            MealSuggestionID = mealSuggestion.Id;
            KidId = mealSuggestion.KidId;
            ConfirmedBy = confirmedBy;
            ConfirmedOn = DateTime.UtcNow;
            WasSuggestionSelected = true;
            MealSuggestion = mealSuggestion;

            if (!string.IsNullOrWhiteSpace(alternateMealName))
            {
                AlternateMealName = alternateMealName;
                AlternateMealDescription = alternateMealDescription;
                WasSuggestionSelected = false;
            }
        }
    }
}