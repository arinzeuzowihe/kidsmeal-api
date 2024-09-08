using System.Diagnostics.CodeAnalysis;
using KidsMealApi.DataAccess.Models;
using KidsMealApi.Modules.Enums;
using KidsMealApi.Modules.Interfaces;

namespace KidsMealApi.Modules.Meals.Core
{
    public class MealHistoryResponse : IClientResponse
    {
        public MealHistoryResponse(IEnumerable<MealHistoryEntry> mealHistoryEntries, ClientResponseErrorCodes? errorCode = null)
        {
            if (mealHistoryEntries == null || !mealHistoryEntries.Any())
            {
                EatingHistory = new Dictionary<int, List<MealHistoryEntry>>();
                return;
            }

            EatingHistory = mealHistoryEntries.GroupBy(k => k.KidID).ToDictionary(g => g.Key, g => g.ToList());

            if(errorCode != null)
            {
                ErrorCode = errorCode;
            }
        }

        [AllowNull]
        public Dictionary<int, List<MealHistoryEntry>> EatingHistory { get; set; }
        public ClientResponseErrorCodes? ErrorCode { get; set; }
    }

    public class MealHistoryEntry
    {
        const string NOT_AVAILABLE = "N/A";
        public MealHistoryEntry(int kidID, string mealName, string mealDescription, string mealType, DateTime? eatenOn)
        {
            KidID = kidID;
            MealName = mealName ?? NOT_AVAILABLE;
            MealDescription = mealDescription ?? NOT_AVAILABLE;
            MealType = mealType ?? NOT_AVAILABLE;
            if (!eatenOn.HasValue)
            {
                EatenOn = NOT_AVAILABLE;
            }
            else if (eatenOn.Value.Kind == DateTimeKind.Utc)
            {
                //TODO: User the user's timezone to do conversion
                var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(eatenOn.Value, TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time"));
                EatenOn = localDateTime.ToShortDateString();
            }
            else 
            {
                EatenOn = eatenOn.Value.ToShortDateString();
            }
        }
        public int KidID { get; set; }

        public string MealName { get; set; }

        public string MealDescription { get; set; }

        public string MealType { get; set; }

        public string EatenOn { get; set; }
    }

}