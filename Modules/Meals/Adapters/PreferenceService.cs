using KidsMealApi.DataAccess;
using KidsMealApi.DataAccess.Models;
using KidsMealApi.Modules.Meals.Ports;
using Microsoft.EntityFrameworkCore;

namespace KidsMealApi.Modules.Meals.Adapters
{
    public class PreferenceService : BaseDataAccessService<MealPreference>, IPreferenceService
    {
        public PreferenceService(KidsMealDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Meal>> AddPreferenceAsync(IEnumerable<int> kidIds, int mealID, IEnumerable<MealType> types)
        {
            if (kidIds == null || !kidIds.Any())
                throw new ArgumentNullException(nameof(kidIds));
            
            if (types == null || !types.Any())
                throw new ArgumentNullException(nameof(types));
            
            foreach(var kidId in kidIds)
            {
                foreach (var mealType in types)
                {
                    await CreateAsync(new MealPreference(mealID, kidId, mealType, true), false);
                }
            }

            await _dbContext.SaveChangesAsync();

            return GetPreferredMeals(kidIds);
        }

        public IEnumerable<MealPreference> GetPreferences(IEnumerable<int> kidIds, bool activeOnly = true)
        {
            if (kidIds == null)
                throw new ArgumentNullException(nameof(kidIds));
            var finalizedKidIds = kidIds.ToList();
            IQueryable<MealPreference> preferences = GetAll().Include(mp => mp.Meal);
            if (finalizedKidIds.Count == 1)
                preferences = preferences.Where(mp => mp.KidId == kidIds.FirstOrDefault());
            else if (finalizedKidIds.Count > 1)
                preferences = preferences.Where(mp => kidIds.Contains(mp.KidId));
            
            if (activeOnly)
                preferences = preferences.Where(mp => mp.IsActive);

            return preferences.AsEnumerable();
        }

        public IEnumerable<Meal> GetPreferredMeals(IEnumerable<MealPreference> preferences)
        {
            if (preferences == null)
                throw new ArgumentNullException(nameof(preferences));

            var distinctKidIds = preferences.Select(p => p.KidId).Distinct().ToList();
            if (distinctKidIds.Count == 0)
                return new List<Meal>();

            //Do not assume preferences has navigation props populated
            var preferencesWithMeals = GetAll().Where(mp => preferences.Select(p => p.Id).Contains(mp.Id))
                                                .Include(mp => mp.Meal).ToList();

            if (distinctKidIds.Count == 1)
                return preferencesWithMeals.Select(mp => mp.Meal).Distinct();
            
             //Get all preferences for kids grouped by meal and type
            var preferencesByMealAndType = preferencesWithMeals.GroupBy(mp => new { mp.Meal, mp.MealType, mp.IsActive })
                                            .Select(g => new
                                            {
                                                Meal = g.Key.Meal,
                                                MealType = g.Key.MealType,
                                                TotalKids = g.Count()
                                            });

            //Filter our preferences that are not common to all kids and group by meal
            //A dictionary of meals common for all children and the types that are common for all children as well
            return preferencesByMealAndType.Where(gp => gp.TotalKids == distinctKidIds.Count())
                                           .Select(gp => gp.Meal).Distinct();
        }

        public IEnumerable<Meal> GetPreferredMeals(IEnumerable<int> kidIds, bool activeOnly = true)
        {        
            var preferences = GetPreferences(kidIds.ToList(), activeOnly);
            return GetPreferredMeals(preferences);
        }

        public async Task<IEnumerable<Meal>> RemovePreferenceAsync(IEnumerable<int> kidIds, int mealID, IEnumerable<MealType>? mealTypes = null)
        {
            var mealPreferencesToRemove = GetAll().Where(mp => mp.MealId == mealID && kidIds.Contains(mp.KidId));

            if (mealTypes != null && mealTypes.Any())
                mealPreferencesToRemove = mealPreferencesToRemove.Where(mp => mealTypes.Contains(mp.MealType));

            await updateActiveStatusAsync(mealPreferencesToRemove.ToList(), false);
            return GetPreferredMeals(kidIds);
        }

        public async Task<IEnumerable<Meal>> UpdatePreferenceAsync(IEnumerable<int> kidIds, int mealID, bool? isActive = null, IEnumerable<MealType>? types = null)
        {
            if (!isActive.HasValue && types == null) //nothing to update on the preference level
                return GetPreferredMeals(kidIds);
            
            //Retrieve both active and inactive meal preferences
            var mealPreferencesToUpdate = GetAll().Where(mp => mp.MealId == mealID && kidIds.Contains(mp.KidId)).ToList();
            if (!mealPreferencesToUpdate.Any())
                return GetPreferredMeals(kidIds);

            //Set all meal preferences to the same active status (including ones that will inevitably be in active)
            if (isActive.HasValue)
                await updateActiveStatusAsync(mealPreferencesToUpdate.ToList(), isActive.Value);

            //Reconcile meal preference's active status based on meal type 
            if (types != null && types.Any())
            {
                var existingMealTypes = mealPreferencesToUpdate.Select(mp => mp.MealType).Distinct().ToList();
                var removedMealTypes = existingMealTypes.Except(types);
                var addedMealTypes = types.Except(existingMealTypes);

                if (removedMealTypes != null && removedMealTypes.Any())
                    await RemovePreferenceAsync(kidIds, mealID, removedMealTypes);

                if (addedMealTypes != null && addedMealTypes.Any())
                    await AddPreferenceAsync(kidIds, mealID, addedMealTypes);
            }

            return GetPreferredMeals(kidIds);
        }

        protected override IQueryable<MealPreference> GetAll()
        {
            return _dbContext.MealPreferences;
        }

        public Dictionary<Meal, IEnumerable<MealType>> GetAllPreferredMealDetails(IEnumerable<int> kidIds)
        {

            if (kidIds == null)
                throw new ArgumentNullException(nameof(kidIds));

            return getMealTypeOccurrences(kidIds);
        }

        public Dictionary<Meal, IEnumerable<MealType>> GetAllPreferredMealDetails(int kidID)
        {
            return getMealTypeOccurrences(new List<int> { kidID });
        }

        public (Meal Meal, IEnumerable<MealType> MealTypes) GetPreferredMealDetails(IEnumerable<int> kidIds, int mealID)
        {
            var preferenceKvp = getMealTypeOccurrences(kidIds, mealID).Single();
            return (preferenceKvp.Key, preferenceKvp.Value);
        }

        private Dictionary<Meal, IEnumerable<MealType>> getMealTypeOccurrences(IEnumerable<int> kidIds, int? mealID = null)
        {
            if (kidIds == null)
                throw new ArgumentNullException(nameof(kidIds));

            if (kidIds.Count() == 1)
            {
                var kidID = kidIds.FirstOrDefault();
                return GetAll().Include(mp => mp.Meal).Where(mp => mp.KidId == kidID && mp.MealId == (mealID ?? mp.MealId)).ToList()
                            .GroupBy(mp => mp.Meal)
                            .Select(mp => new
                            {
                                Meal = mp.Key,
                                MealTypes = mp.Select(m => m.MealType)
                            })
                            .ToDictionary(c => c.Meal, c => c.MealTypes);
            }

            //Get all preferences for kids grouped by meal and type
            var preferencesByMealAndType = GetAll().Include(mp => mp.Meal).Where(mp => kidIds.Contains(mp.KidId) && mp.MealId == (mealID ?? mp.MealId)).ToList()
                                                    .GroupBy(mp => new { mp.Meal, mp.MealType })
                                                    .Select(g => new {
                                                        Meal = g.Key.Meal,
                                                        MealType = g.Key.MealType,
                                                        TotalKids = g.Count()
                                                    });

            //Filter our preferences that are not common to all kids and group by meal
            //A dictionary of meals common for all children and the types that are common for all children as well
            return preferencesByMealAndType.Where(gp => gp.TotalKids == kidIds.Count())
                                            .GroupBy(gp => gp.Meal)
                                            .Select(gp => new
                                            {
                                                Meal = gp.Key,
                                                MealTypes = gp.Select(g => g.MealType)
                                            })
                                            .ToDictionary(c => c.Meal, c => c.MealTypes);
        }

        private async Task updateActiveStatusAsync(List<MealPreference> mealPreferencesToUpdate, bool isActive)
        {
            foreach (var mealPreference in mealPreferencesToUpdate)
            {
                await UpdateAsync(mealPreference, (preference) =>
                {
                    preference.IsActive = isActive;
                }, false);
            }
            
            await _dbContext.SaveChangesAsync();
        }
    }
}