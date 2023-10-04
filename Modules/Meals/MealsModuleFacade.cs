using KidsMealApi.Modules.Meals.Ports;

namespace KidsMealApi.Modules.Meals
{
    public class MealsModuleFacade
    {
        public MealsModuleFacade(IPreferenceService preferenceService,
                                IMealService mealService,
                                ISuggestionService suggestionService,
                                IHistoryService historyService)
        {
            PreferenceService = preferenceService;
            MealService = mealService;
            SuggestionService = suggestionService;
            HistoryService = historyService;
        }

        public IPreferenceService PreferenceService { get; set; }
        public IMealService MealService { get; set; }
        public ISuggestionService SuggestionService { get; set; }
        public IHistoryService HistoryService { get; set; }
    }
}