using System.Diagnostics.CodeAnalysis;
using KidsMealApi.DataAccess.Models;

namespace KidsMealApi.Modules.Meals.Core
{
    public class AddPreferenceRequest
    {
        [AllowNull]
        public IEnumerable<int> KidIds { get; set; }
        [AllowNull]
        public string MealName { get; set; }
        [AllowNull]
        public string MealDescription { get; set; }
        public bool IsSideDish { get; set; }
        public bool IsTakeout { get; set; }
        [AllowNull]
        public IEnumerable<MealType> MealTypes {get; set; }
    }
}