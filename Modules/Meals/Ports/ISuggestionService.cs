using KidsMealApi.DataAccess.Models;

namespace KidsMealApi.Modules.Meals.Ports
{
    public interface ISuggestionService
    {
        /// <summary>
        /// Generates a meal suggestion for the user without persist it to the data store.
        /// </summary>
        /// <param name="mealSuggestionIDs"></param>
        /// <returns></returns>
        IEnumerable<MealSuggestion> GenerateNextMealSuggestion(IEnumerable<int> kidIDs, MealType mealType, IEnumerable<MealHistory> recentHistories, IEnumerable<MealPreference> preferences, bool enforceSameSuggestion = false);
        /// <summary>
        /// Retrieves existing meal suggestions from the data store.
        /// </summary>
        /// <param name="mealSuggestionIDs"></param>
        /// <returns></returns>
        IEnumerable<MealSuggestion> GetMealSuggestions(IEnumerable<int> mealSuggestionIDs);
        /// <summary>
        /// Persists a meal suggestion to the data store.
        /// </summary>
        /// <param name="kidIDs"></param>
        /// <param name="recentHistories"></param>
        /// <param name="preferences"></param>
        /// <returns></returns>
        Task<IEnumerable<MealSuggestion>> SaveSuggestionsAsync(IEnumerable<MealSuggestion> mealSuggestions);
        /// <summary>
        /// Retrieves existing unconfirmed meal suggestions.
        /// </summary>
        /// <param name="kidIDs"></param>
        /// <returns></returns>
        Task<IEnumerable<MealSuggestion>> GetPendingSuggestionsAsync(IEnumerable<int> kidIDs);
        /// <summary>
        /// Soft deletes existing meal suggestions.
        /// </summary>
        /// <param name="mealSuggestionIDs"></param>
        /// <returns></returns>
        Task<bool> DeleteSuggestionsAsync(IEnumerable<int> mealSuggestionIDs);
    }
}