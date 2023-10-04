using KidsMealApi.DataAccess.Models;

namespace KidsMealApi.Modules.Meals.Ports
{
    public interface IHistoryService
    {
        Task<(int Success, int Failed)> CreateMealHistoryAsync(IEnumerable<MealHistory> histories);
        IEnumerable<MealHistory> GetMealHistory(int kidID, MealType? mealType = null);
        IEnumerable<MealHistory> GetMealHistory(IEnumerable<int> kidIDs, MealType? mealType = null);
    }
}