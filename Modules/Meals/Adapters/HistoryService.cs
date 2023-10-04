using KidsMealApi.DataAccess;
using KidsMealApi.DataAccess.Models;
using KidsMealApi.Modules.Meals.Ports;
using Microsoft.EntityFrameworkCore;

namespace KidsMealApi.Modules.Meals.Adapters
{
    public class HistoryService : BaseDataAccessService<MealHistory>, IHistoryService
    {
        public HistoryService(KidsMealDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<(int Success, int Failed)> CreateMealHistoryAsync(IEnumerable<MealHistory> histories)
        {
            var successfulConfirmations = 0;
            var failedConfirmations = 0;
            if (histories == null || !histories.Any())
                return(successfulConfirmations, failedConfirmations);
            
            foreach (var history in histories)
            {
                try
                {
                    var suggestion = history.MealSuggestion;
                    //Check if suggestion was already confirmed
                    if (suggestion == null || suggestion.IsConfirmed)
                    {
                        //TODO: Log that that the suggestion was already confirmed
                        continue;
                    }

                    //Update meal suggestion nav property to confirm meal suggestion
                    suggestion.IsConfirmed = true;

                    //Record confirmation and save any nav property changes
                    await CreateAsync(history);
                    successfulConfirmations++;
                }
                catch (System.Exception)
                {
                    //TODO: Log failure
                    failedConfirmations++;
                }
            }

            return (successfulConfirmations, failedConfirmations);
        }

        public IEnumerable<MealHistory> GetMealHistory(int kidID, MealType? mealType = null)
        {
            var mealHistory = GetAll().Include(mh => mh.MealSuggestion)
                                        .Where(mh => mh.KidId == kidID);

            if (mealType.HasValue)
                mealHistory = mealHistory.Where(mh => mh.MealSuggestion.MealType == mealType.Value);

            return mealHistory;

        }

        public IEnumerable<MealHistory> GetMealHistory(IEnumerable<int> kidIDs, MealType? mealType = null)
        {
            var mealHistory = GetAll().Include(mh => mh.MealSuggestion)
                                        .Where(mh => kidIDs.Contains(mh.KidId));
            if (mealType.HasValue)
                mealHistory = mealHistory.Where(mh => mh.MealSuggestion.MealType == mealType.Value);

            return mealHistory.ToList();
        }

        protected override IQueryable<MealHistory> GetAll()
        {
            return _dbContext.MealHistories;
        }
    }
}