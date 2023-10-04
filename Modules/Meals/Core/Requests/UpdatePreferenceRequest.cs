namespace KidsMealApi.Modules.Meals.Core
{
    public class UpdatePreferenceRequest : AddPreferenceRequest
    {
        public int MealId { get; set; }
        public bool IsActive { get; set; }
    }
}