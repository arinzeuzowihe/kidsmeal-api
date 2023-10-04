using KidsMealApi.DataAccess;
using KidsMealApi.DataAccess.Models;
using KidsMealApi.Modules.Meals.Ports;

namespace KidsMealApi.Modules.Meals.Adapters
{
    public class SuggestionService : BaseDataAccessService<MealSuggestion>, ISuggestionService
    {
        public SuggestionService(KidsMealDbContext dbContext) : base(dbContext)
        {
        }

        ///<inheritdoc>
        public IEnumerable<MealSuggestion> GenerateNextMealSuggestion(IEnumerable<int> kidIDs, MealType mealType, IEnumerable<MealHistory> recentHistories, IEnumerable<MealPreference> preferences)
        {
            var generatedMealSuggestions = new List<MealSuggestion>();
            foreach(var kidID in kidIDs)
            {
                //Get history for kid
                var mealHistories = recentHistories.Where(rh => rh.KidId == kidID);

                //Get Preferences for kid
                var mealPreferences = preferences.Where(p => p.KidId == kidID);
                if (!mealPreferences.Any())
                {
                    //TODO: Log "no preferences found for kid"
                    continue;
                }

                //Get next meal suggestion
                var suggestedMeal = getNextMealSuggestion(mealHistories, mealPreferences);

                generatedMealSuggestions.Add(new MealSuggestion(kidID, suggestedMeal, mealType));
            }

            return generatedMealSuggestions;
        }

        public async Task<IEnumerable<MealSuggestion>> SaveSuggestionsAsync(IEnumerable<int> kidIDs, MealType mealType, IEnumerable<MealHistory> recentHistories, IEnumerable<MealPreference> preferences)
        {
            var createdMealSuggestions = new List<MealSuggestion>();
            foreach(var kidID in kidIDs)
            {
                //Get history for kid
                var mealHistories = recentHistories.Where(rh => rh.KidId == kidID);

                //Get Preferences for kid
                var mealPreferences = preferences.Where(p => p.KidId == kidID);
                if (!mealPreferences.Any())
                {
                    //TODO: Log "no preferences found for kid"
                    continue;
                }

                //Get next meal suggestion
                var suggestedMeal = getNextMealSuggestion(mealHistories, mealPreferences);

                //Persist and track created meal suggestion
                var createdSuggestion = await CreateAsync(new MealSuggestion(kidID, suggestedMeal, mealType), false);
                createdMealSuggestions.Add(createdSuggestion);
            }

            await _dbContext.SaveChangesAsync();

            return createdMealSuggestions;
        }

        public async Task<bool> DeleteSuggestionsAsync(IEnumerable<int> mealSuggestionIDs)
        {
            var pendingSuggestions = GetAll().Where(ms => mealSuggestionIDs.Contains(ms.Id) && !ms.IsConfirmed);
            if (pendingSuggestions == null || !pendingSuggestions.Any())
                return true;
            
            foreach (var suggestion in pendingSuggestions)
            {
                await DeleteAsync(suggestion, false);
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public IEnumerable<MealSuggestion> GetMealSuggestions(IEnumerable<int> mealSuggestionIDs)
        {
            if (mealSuggestionIDs == null || !mealSuggestionIDs.Any())
                return new List<MealSuggestion>();

            return GetAll().Where(ms => mealSuggestionIDs.Contains(ms.Id));
        }

        public async Task<IEnumerable<MealSuggestion>> GetPendingSuggestionsAsync(IEnumerable<int> kidIDs)
        {
            //Please Note a single kid should NEVER have more than 1 uncofirmed suggesstion at a time
            var pendingSuggestionsGroupedByKid = GetAll().Where(ms => !ms.IsConfirmed && kidIDs.Contains(ms.KidId))
                                                    .ToList()
                                                    .GroupBy(ms => ms.KidId);


            var pendingSuggestionsGroups = pendingSuggestionsGroupedByKid.Select(msg => new
            {
                Latest = msg.OrderByDescending(ms => ms.CreatedOn).First(),
                HasMultiple = msg.Count() > 1,
                Suggestions = msg.ToList()
            });

            //Handle errorneous scenarios where kid has multiple unconfirmed suggestions
            foreach (var group in pendingSuggestionsGroups.Where(psg => psg.HasMultiple))
            {
                //For now auto-confirm these
                var remainingSuggestionIDs = group.Suggestions.Where(s => s.Id != group?.Latest?.Id)
                                                                .Select(s => s.Id);

                await confirmSuggestionsAsync(remainingSuggestionIDs);

                //TODO: Log auto-confirmation
            }

            return pendingSuggestionsGroups.Select(psg => psg.Latest);
        }

        protected override IQueryable<MealSuggestion> GetAll()
        {
            return _dbContext.MealSuggestions;
        }

        #region Private Helper Methods
        private Meal getNextMealSuggestion(IEnumerable<MealHistory> histories, IEnumerable<MealPreference> preferences)
        {
            if (preferences == null || !preferences.Any())
                throw new ArgumentNullException("Cannot determine a meal suggestion because there are no preference(s).");

            //All food that have not been eaten recently
            var namesOfPastMeals = histories.Select(h => h.AlternateMealID.HasValue ? h.AlternateMealName : h.MealSuggestion?.MealName);
            var eligibleSuggestions = preferences.Where(p => !namesOfPastMeals.Contains(p.Meal?.Name)).ToList();

            if (eligibleSuggestions == null || !eligibleSuggestions.Any())
                throw new ArgumentNullException("Cannot determine a meal suggestion because there is not enough preferences.");


            var nextSuggestionIndex = Random.Shared.Next(0, (eligibleSuggestions.Count() - 1));
            var nextSuggestion = eligibleSuggestions[nextSuggestionIndex];
            if (nextSuggestion?.Meal == null || string.IsNullOrWhiteSpace(nextSuggestion.Meal.Name))
                throw new Exception("There was only one suggestion but unable to determine the name of the meal.");

            return nextSuggestion.Meal;

        }

        private async Task<(List<int> ConfirmedIDs, List<int> FailedIDs)> confirmSuggestionsAsync(IEnumerable<int> mealSuggestionIDs)
        {
            var confirmedMealSuggestionIDs = new List<int>();
            var failedMealSuggestionIDs = new List<int>();
            if (mealSuggestionIDs == null || !mealSuggestionIDs.Any())
                return (confirmedMealSuggestionIDs, failedMealSuggestionIDs);

            var pendingSuggestions = GetAll().Where(ms => mealSuggestionIDs.Contains(ms.Id));

            foreach (var suggestion in pendingSuggestions)
            {
                try
                {
                    //Check if suggestion was already confirmed
                    if (suggestion.IsConfirmed)
                    {
                        //TODO: Log that that the suggestion was already confirmed
                        continue;
                    }

                    await UpdateAsync(suggestion, (ms) =>
                    {
                        ms.IsConfirmed = true;
                    }, false);
                    confirmedMealSuggestionIDs.Add(suggestion.Id);
                }
                catch (System.Exception)
                {
                    //TODO: Log failure
                    failedMealSuggestionIDs.Add(suggestion.Id);
                }
            }

            await _dbContext.SaveChangesAsync();

            return (confirmedMealSuggestionIDs, failedMealSuggestionIDs);
        }

        public async Task<IEnumerable<MealSuggestion>> SaveSuggestionsAsync(IEnumerable<MealSuggestion> mealSuggestions)
        {
            var savedMealSuggestions = new List<MealSuggestion>();
            foreach(var mealSuggestion in mealSuggestions)
            {
                var createdSuggestion = await CreateAsync(mealSuggestion, false);
                savedMealSuggestions.Add(createdSuggestion);
            }

            await _dbContext.SaveChangesAsync();

            return savedMealSuggestions;
        }

        #endregion
    }
}