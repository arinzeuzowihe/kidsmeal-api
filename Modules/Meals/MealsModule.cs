using KidsMealApi.DataAccess;
using KidsMealApi.DataAccess.Models;
using KidsMealApi.Modules.Interfaces;
using KidsMealApi.Modules.Meals;
using KidsMealApi.Modules.Meals.Adapters;
using KidsMealApi.Modules.Meals.Core;
using KidsMealApi.Modules.Meals.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class MealsModule : IModule
{
    private int _mealHistoryThresholdInDays { get; set; }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        //Retrieve all preferences common to a group of kids or a single child
        endpoints.MapPost("/meal/preferences/{activeOnly}", [Authorize] (List<int> kidIDs, bool activeOnly, [FromServices] MealsModuleFacade facade) => {

           var preferences = facade.PreferenceService.GetPreferences(kidIDs, activeOnly).ToList();
           var preferredMeals = facade.PreferenceService.GetPreferredMeals(preferences).ToList();

            var basicMealPreferences = (from preference in preferences
                                        join meal in preferredMeals on preference.MealId equals meal.Id
                                        select new
                                        {
                                            MealId = meal.Id,
                                            MealName = meal.Name,
                                            IsActive = preference.IsActive
                                        }).Distinct();
            

            return Results.Ok(basicMealPreferences.Distinct());
        });

        //Retrieve preferred meal details common to a group of kids or a single child
        endpoints.MapPost("/meal/preference/{mealId}", [Authorize] (List<int> kidIDs, int mealId, [FromServices] MealsModuleFacade facade) => {

            var preferredMealDetails = facade.PreferenceService.GetPreferredMealDetails(kidIDs, mealId);
            var meal = preferredMealDetails.Meal;
            var mealTypes = preferredMealDetails.MealTypes;

            if (meal == null)
                return Results.NotFound();

            var finalizedDetails = new {
                                            MealId = meal.Id,
                                            MealName = meal.Name,
                                            MealDescription = meal.Description,
                                            MealTypes = mealTypes,
                                            isSideDish = meal.IsSideDish,
                                            IsTakeout = meal.IsTakeout
                                        };

            return Results.Ok(finalizedDetails);
        });

        //Add new preference for kid(s)
        endpoints.MapPost("/meal/preferences", [Authorize] async (AddPreferenceRequest request, [FromServices] MealsModuleFacade facade) => {
            
            if (request == null)
                return Results.BadRequest();

            //Create meal
            var meal = new Meal
            {
                Name = request.MealName,
                Description = request.MealDescription,
                IsSideDish = request.IsSideDish,
                IsTakeout = request.IsTakeout,
                CreatedOn = DateTime.UtcNow
        };
            var mealID = await facade.MealService.CreateMealAsync(meal);

            //Associate meal to kid preference
            var updatedMeals = await facade.PreferenceService.AddPreferenceAsync(request.KidIds, mealID, request.MealTypes);
            
            if (!updatedMeals.Any())
                return Results.Created($"/meal/{mealID}", meal); //if we cannot return preferences; at least indicate that the meals was created

            return Results.Ok(new PreferredMealsResponse(updatedMeals));
        });

        endpoints.MapPut("/meal/preferences", [Authorize] async (UpdatePreferenceRequest request, [FromServices] MealsModuleFacade facade) => {

            if (request == null)
                return Results.BadRequest();

            if (request.MealId <= 0)
                return Results.BadRequest("Invalid Meal");
            
            await facade.MealService.UpdateMealAsync(new Meal
                                                            {
                                                                Id = request.MealId,
                                                                Name = request.MealName,
                                                                Description = request.MealDescription,
                                                                IsSideDish = request.IsSideDish,
                                                                IsTakeout = request.IsTakeout
                                                            });

            var updatedMeals = await facade.PreferenceService.UpdatePreferenceAsync(request.KidIds, request.MealId, request.IsActive, request.MealTypes);
            return Results.Ok(new PreferredMealsResponse(updatedMeals));

        });

        endpoints.MapDelete("/meal/preferences", [Authorize] async ([FromBody]RemovePreferenceRequest request, [FromServices] MealsModuleFacade facade) => {
            
            if (request == null)
                return Results.BadRequest();

            var updatedMeals = await facade.PreferenceService.RemovePreferenceAsync(request.kidIDs, request.MealID);
            return Results.Ok(new PreferredMealsResponse(updatedMeals));
        });

        endpoints.MapGet("/meal/{mealID}", [Authorize] async (int mealID, [FromServices] MealsModuleFacade facade) =>
        {
            var meal = await facade.MealService.GetMealByIDAsync(mealID);
            if (meal == null)
                return Results.NotFound();

            return Results.Ok(new { MealId = meal.Id, MealName = meal.Name, MealDescription = meal.Description });
        });

        endpoints.MapPost("/meal/suggestion/generate", [Authorize] (GenerateSuggestionRequest request, [FromServices] MealsModuleFacade facade) => {
            if (request == null)
                return Results.BadRequest();

            //Get last 3 days worth of history
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-3);
            var recentHistories = (facade.HistoryService.GetMealHistory(request.KidIDs, request.MealType)).Where(h => h.ConfirmedOn > startDate && endDate > h.ConfirmedOn);
            var currentPreferences = facade.PreferenceService.GetPreferences(request.KidIDs).Where(p => p.MealType == request.MealType);
            if (!request.IncludeTakeOut)
                currentPreferences = currentPreferences.Where(p => !p.Meal.IsTakeout);

            var mealSuggestions = facade.SuggestionService.GenerateNextMealSuggestion(request.KidIDs, request.MealType, recentHistories, currentPreferences.ToList());
            return Results.Ok(new PendingMealSuggestionResponse(mealSuggestions));
        } );

        endpoints.MapPost("/meal/suggestion/pending", [Authorize] async (List<int> kidIDs,  [FromServices] MealsModuleFacade facade) => {

            var pendingSuggestions = await facade.SuggestionService.GetPendingSuggestionsAsync(kidIDs);
            return Results.Ok(new PendingMealSuggestionResponse(pendingSuggestions));
        } );

        endpoints.MapPost("/meal/suggestion", [Authorize] async (SaveMealSuggestionRequest request, [FromServices] MealsModuleFacade facade) => {
            
            if (request?.MealSuggestions == null || !request.MealSuggestions.Any())
                throw new ArgumentNullException(nameof(request));

            var kidIds = request.MealSuggestions.Select(m => m.KidId);
            //Check if there are any unconfirmed suggestions (if so return those)
            var pendingSuggestions = await facade.SuggestionService.GetPendingSuggestionsAsync(kidIds);
            if (pendingSuggestions.Any() && !request.IgnorePendingSuggestions)
                return Results.Ok(new PendingMealSuggestionResponse(pendingSuggestions));
            
            if (pendingSuggestions.Any() && request.IgnorePendingSuggestions)
                await facade.SuggestionService.DeleteSuggestionsAsync(pendingSuggestions.Select(ps => ps.Id));

            //Persist the meal suggestions
            var savedMealSuggestions = await facade.SuggestionService.SaveSuggestionsAsync(request.MealSuggestions);
            return Results.Ok(new PendingMealSuggestionResponse(savedMealSuggestions));
        } );

        endpoints.MapPost("/meal/suggestion/review", [Authorize] async (ReviewMealSuggestionRequest request, [FromServices] MealsModuleFacade facade) => {
            
            if (request?.Reviews == null || !request.Reviews.Any())
                throw new ArgumentNullException(nameof(request));

            var mealSuggestionIDs = request.Reviews.Select(sms => sms.MealSuggestionID);
            var alternateMealIDs = request.Reviews.Where(r => r.AlternateMealID.HasValue)
                                                  .Select(fr => fr.AlternateMealID.GetValueOrDefault()).ToList();

            var existingAlternateMeals = facade.MealService.GetMealsByID(alternateMealIDs);
            var existingMealSuggestions = facade.SuggestionService.GetMealSuggestions(mealSuggestionIDs).ToList();

            var mealHistoriesWithUnconfirmedSuggestions = new List<MealHistory>();
            foreach (var review in request.Reviews)
            {
                var mealSuggestion = existingMealSuggestions.FirstOrDefault(cms => review.MealSuggestionID == cms.Id);
                if (mealSuggestion == null)
                {
                    //TODO: log an exception missing suggestion
                    continue;
                }

                Meal? alternateMeal = null;
                if (review.AlternateMealID.HasValue)
                {
                    //The alternate meal selected by the user for an existing meal in the application
                    alternateMeal = existingAlternateMeals.FirstOrDefault(e => e.Id == review.AlternateMealID.GetValueOrDefault());
                }
                else if (!string.IsNullOrWhiteSpace(review.AlternateMealName) || !string.IsNullOrWhiteSpace(review.AlternateMealDescription))
                {
                    //TODO: log a not supported exception
                    //because we do not support user-entered alternate meals yet
                    continue;
                }
                var mealHistory = new MealHistory(mealSuggestion, request.UserID, alternateMeal);

                mealHistoriesWithUnconfirmedSuggestions.Add(mealHistory);
            }                                                                                                        

            //Confirm meal suggestion and record user meal selection in history
            var results = await facade.HistoryService.CreateMealHistoryAsync(mealHistoriesWithUnconfirmedSuggestions);
            return Results.Ok(new { Confirmed = results.Success, Failed = results.Failed  });

        } );
        
        endpoints.MapPost("/meal/history", [Authorize] (MealHistoryRequest request, [FromServices] MealsModuleFacade facade) => {
            
            if (request?.KidIDs == null)
                return Results.BadRequest();

            //Retrieve history from x days from today
            var endDateTime = DateTime.Now;
            var startDateTime = endDateTime.AddDays(-(request.DaysFromToday ?? _mealHistoryThresholdInDays));

            var histories = facade.HistoryService.GetMealHistory(request.KidIDs).Where(h=> h.ConfirmedOn >= new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day) && h.ConfirmedOn <= endDateTime );
            if (histories == null || !histories.Any())
                return Results.NoContent();

            var finalizedHistory = histories.Select(h => new MealHistoryEntry (
                h.KidId,
                ((h.WasSuggestionSelected) ? h.MealSuggestion.MealName : h.AlternateMealName) ?? "N/A",
                ((h.WasSuggestionSelected) ? h.MealSuggestion.MealDescription : h.AlternateMealDescription) ?? "N/A",
                (h.MealSuggestion != null) ? h.MealSuggestion.MealType.ToString() : "N/A",
                (h.MealSuggestion != null) ? h.MealSuggestion.CreatedOn.ToShortDateString() : "N/A"
            ));

            return Results.Ok(new MealHistoryResponse(finalizedHistory));

        } );
        
        return endpoints;
    }

    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IPreferenceService, PreferenceService>();
        services.AddTransient<IMealService, MealService>();
        services.AddTransient<ISuggestionService, SuggestionService>();
        services.AddTransient<IHistoryService, HistoryService>();
        services.AddTransient<MealsModuleFacade>();

        //store settings
        _mealHistoryThresholdInDays = int.TryParse(configuration["Meal:HistoryThresholdInDays"], out int parsedSetting) ? parsedSetting : 7;

        return services;
    }

}