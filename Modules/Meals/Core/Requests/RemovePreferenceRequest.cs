using System.Diagnostics.CodeAnalysis;

namespace KidsMealApi.Modules.Meals.Core
{
    public class RemovePreferenceRequest
    {
        [AllowNull]
        public IEnumerable<int> kidIDs { get; set; }

        public int MealID { get; set; }
    }
}