using KidsMealApi.DataAccess.Models;

namespace KidsMealApi.Modules.Meals.Ports
{
    public interface IMealService
    {
        Task<int> CreateMealAsync(Meal meal);
        Task<Meal> UpdateMealAsync(Meal meal);
        Task<Meal> GetMealByIDAsync(int mealID);
        List<Meal> GetMealsByID(List<int> mealIds);
        Task<bool> DeleteMealAsync(int mealID);
    }
}