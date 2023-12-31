using KidsMealApi.DataAccess;
using KidsMealApi.DataAccess.Models;

namespace KidsMealApi.Modules.Meals.Ports
{
    public interface IPreferenceService
    {
        /// <summary>
        /// Get all the meals that a kid prefers to eat.
        /// </summary>
        /// <param name="kidID"></param>
        /// <returns></returns>
        IEnumerable<MealPreference> GetPreferences(IEnumerable<int> kidIDs, bool activeOnly = true, bool commonOnly = false);
        /// <summary>
        /// Get all the meals that a kid prefers to eat.
        /// </summary>
        /// <param name="kidID"></param>
        /// <returns></returns>
        IEnumerable<Meal> GetPreferredMeals(IEnumerable<MealPreference> preferences);
        /// <summary>
        /// Get all the common meals that a group of kids prefer to eat.
        /// </summary>
        /// <param name="kidIds"></param>
        /// <returns></returns>
        IEnumerable<Meal> GetPreferredMeals(IEnumerable<int> kidIds, bool activeOnly = true);
        /// <summary>
        /// Gets all the meal types for a specific meal that a kid prefers to eat.
        /// </summary>
        /// <param name="kidIds"></param>
        /// <param name="mealID"></param>
        /// <returns></returns>
        (Meal Meal, IEnumerable<MealType> MealTypes) GetCommonMealWithMealTypes(IEnumerable<int> kidIds, int mealID);
        /// <summary>
        /// Gets all the meal types for each of the meals that a kid prefers to eat.
        /// </summary>
        /// <param name="kidID"></param>
        /// <returns></returns>
        Dictionary<Meal, IEnumerable<MealType>> GetMealsWithMealTypes(int kidID);
        /// <summary>
        /// Gets all the common meal types for each of the meals that a group of kids prefer to eat.
        /// </summary>
        /// <param name="kidIds"></param>
        /// <remarks>Only meal and meal type(s) combination that exactly match the preferences of all kids in the group will be returned.</remarks>
        /// <returns>A dictionary of meals and preferred meal types</returns>
        Dictionary<Meal, IEnumerable<MealType>> GetCommonMealsWithMealTypes(IEnumerable<int> kidIds);
        /// <summary>
        /// Add a meal preference for a children
        /// </summary>
        /// <param name="kidID"></param>
        /// <param name="meal"></param>
        /// <returns></returns>
        Task<IEnumerable<Meal>> AddPreferenceAsync(IEnumerable<int> kidIds, int mealID, IEnumerable<MealType> types);
        /// <summary>
        /// Updates the meal preference for a children
        /// </summary>
        /// <param name="kidID"></param>
        /// <param name="meal"></param>
        /// <returns></returns>
        Task<IEnumerable<Meal>> UpdatePreferenceAsync(IEnumerable<int> kidIds, int mealID, bool? isActive = null, IEnumerable<MealType>? mealTypes = null);
        /// <summary>
        /// Removes a meal preference for a child
        /// </summary>
        /// <param name="kidID"></param>
        /// <param name="mealID"></param>
        /// <returns></returns>
        Task<IEnumerable<Meal>> RemovePreferenceAsync(IEnumerable<int> kidIds, int mealID, IEnumerable<MealType>? mealTypes = null);

    }
}