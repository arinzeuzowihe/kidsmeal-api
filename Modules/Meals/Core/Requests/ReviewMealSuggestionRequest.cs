using System.Diagnostics.CodeAnalysis;

namespace KidsMealApi.Modules.Meals.Core
{
    /// <summary>
    /// A request to provide feedback on the meal suggestion
    /// </summary>
    public class ReviewMealSuggestionRequest
    {
        public int UserID { get; set; }

        [AllowNull]
        public List<MealSuggestionReview> Reviews { get; set; }
    }

    public class MealSuggestionReview
    {
        /// <summary>
        /// The ID of the suggestion generate for the user
        /// </summary>
        public int MealSuggestionID { get; set; }
        /// <summary>
        /// The ID of an existing meal chosen by user instead of the suggestion 
        /// </summary>
        [AllowNull]
        public int? AlternateMealID { get; set; }
        /// <summary>
        /// The name of a non-existing meal chosen by the user instead of the suggestion
        /// </summary>
        [AllowNull]
        public string AlternateMealName { get; set; }
        /// <summary>
        /// The description of a non-existing meal chosen by the user instead of the suggestion
        /// </summary>
        [AllowNull]
        public string AlternateMealDescription { get; set; }
        /// <summary>
        /// A flag to indicate if the child liked the meal.
        /// </summary>
        public bool WasMealLiked { get; set; }
    }
}