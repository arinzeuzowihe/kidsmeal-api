using System.Diagnostics.CodeAnalysis;

namespace KidsMealApi.Modules.Meals.Core
{
    public class MealHistoryRequest
    {
        [AllowNull]
        public List<int> KidIDs { get; set; }
        public int? DaysFromToday { get; set; }
    }
}