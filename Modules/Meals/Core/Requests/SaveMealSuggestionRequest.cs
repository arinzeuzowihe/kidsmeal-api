using System.Diagnostics.CodeAnalysis;
using KidsMealApi.DataAccess.Models;

namespace KidsMealApi.Modules.Meals.Core
{
    /// <summary>
    /// A request to save meal suggestions selected by user.
    /// </summary>
    public class SaveMealSuggestionRequest
    {
        public bool IgnorePendingSuggestions { get; set; }

        [AllowNull]
        public List<MealSuggestion> MealSuggestions { get; set; }
    }
}