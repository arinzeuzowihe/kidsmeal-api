using System.Diagnostics.CodeAnalysis;
using KidsMealApi.DataAccess.Models;

namespace KidsMealApi.Modules.Meals.Core
{
    public class GenerateSuggestionRequest
    {
        [AllowNull]
        public List<int> KidIDs { get; set; }
        public MealType MealType { get; set; }
        public bool IncludeTakeOut { get; set; }
        public bool SameMealForAll { get; set; }
    }
}