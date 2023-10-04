using KidsMealApi.DataAccess;
using KidsMealApi.DataAccess.Models;
using KidsMealApi.Modules.Meals.Ports;

namespace KidsMealApi.Modules.Meals.Adapters
{
    public class MealService : BaseDataAccessService<Meal>, IMealService
    {
        public MealService(KidsMealDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<int> CreateMealAsync(Meal meal)
        {
            var createdMeal = await CreateAsync(meal);
            return createdMeal.Id;
        }

        public async Task<bool> DeleteMealAsync(int mealID)
        {
            try
            {
                await DeleteByIdAsync(mealID);
                return true;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<Meal> GetMealByIDAsync(int mealID)
        {
            return await GetByIdAsync(mealID);
        }

        public List<Meal> GetMealsByID(List<int> mealIds)
        {
            if (mealIds == null || !mealIds.Any()) 
            {
                return new List<Meal>();
            }

            return GetAll().Where(m => mealIds.Contains(m.Id)).ToList();
        }

        public async Task<Meal> UpdateMealAsync(Meal meal)
        {
            if (meal == null)
                throw new ArgumentNullException(nameof(meal));

            return await UpdateAsync(meal.Id, (mealToUpdate) =>
            {
                mealToUpdate.Name = (!string.IsNullOrWhiteSpace(meal.Name) && meal.Name != mealToUpdate.Name) ? meal.Name : mealToUpdate.Name;
                mealToUpdate.Description = (!string.IsNullOrWhiteSpace(meal.Description) && meal.Description != mealToUpdate.Description) ? meal.Description : mealToUpdate.Description;
                mealToUpdate.IsSideDish = (meal.IsSideDish != mealToUpdate.IsSideDish) ? meal.IsSideDish : mealToUpdate.IsSideDish;
                mealToUpdate.IsTakeout = (meal.IsTakeout != mealToUpdate.IsTakeout) ? meal.IsTakeout : mealToUpdate.IsTakeout;
            });
        }

        protected override IQueryable<Meal> GetAll()
        {
            return _dbContext.Meals;
        }
    }
}