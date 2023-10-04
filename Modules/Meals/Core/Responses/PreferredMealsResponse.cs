using System.Diagnostics.CodeAnalysis;
using KidsMealApi.DataAccess.Models;

namespace KidsMealApi.Modules.Meals.Core
{
    public class PreferredMealsResponse
    {
        public PreferredMealsResponse(IEnumerable<Meal> meals)
        {
            MealKeyValuePairs = meals?.ToDictionary(k => k.Id, v => v.Name ?? "N/A") ?? new Dictionary<int, string>();
        }

        public Dictionary<int, string> MealKeyValuePairs { get; set; }
    }
}