using KidsMealApi.DataAccess.Models;
using KidsMealApi.Modules.Enums;
using KidsMealApi.Modules.Interfaces;

namespace KidsMealApi.Modules.Meals.Core
{
    public class PendingMealSuggestionResponse : IClientResponse
    {
        public PendingMealSuggestionResponse(IEnumerable<MealSuggestion> mealSuggestions, ClientResponseErrorCodes? errorCode = null)
        {
            PendingSuggestions = new List<PendingSuggestion>();
            foreach(var mealSuggestion in mealSuggestions)
            {
                PendingSuggestions.Add(new PendingSuggestion(mealSuggestion));
            }

            if (errorCode != null)
            {
                ErrorCode = errorCode;
            }
        }

        public List<PendingSuggestion> PendingSuggestions { get; set; }
        public ClientResponseErrorCodes? ErrorCode { get; set; }
    }

    public class PendingSuggestion
    {
        public PendingSuggestion (MealSuggestion mealSuggestion)
        {
            SuggestionID = mealSuggestion.Id;
            KidId = mealSuggestion.KidId;
            MealName = mealSuggestion.MealName;
            MealDescription = mealSuggestion.MealDescription;
            MealType = mealSuggestion.MealType;
        }

        public int SuggestionID { get; init; }
        public int KidId { get; init; }
        public string MealName { get; init; }
        public string MealDescription { get; set; }
        public MealType MealType { get; set; }
    }

}