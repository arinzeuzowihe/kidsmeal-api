using System.ComponentModel;

namespace KidsMealApi.Modules.Enums
{
    public enum ClientResponseErrorCodes
    {
        [Description("Meal history already exists for meal type on that day.")]
        MEAL_HIST_EXIST
    }
}