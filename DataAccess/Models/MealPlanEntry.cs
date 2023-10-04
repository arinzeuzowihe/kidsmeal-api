using System.ComponentModel.DataAnnotations;

namespace KidsMealApi.DataAccess.Models
{
    public class MealPlanEntry
    {
        public MealPlanEntry()
        {
            
        }

        public MealPlanEntry(Guid mealPlanEntryID, Guid kidID, string mealname)
        {
            MealPlanEntryID = mealPlanEntryID;
            KidID = kidID;
            MealName = mealname;
            CreatedDateTime = DateTime.UtcNow;
        }

        [Key]
        public Guid MealPlanEntryID { get; set; }
        
        public Guid KidID { get; set; }

        public string? MealName { get; set; }

        public DateTime CreatedDateTime  { get; set; }
        
        public DateTime LastUpdated { get; set; }

        public Guid LastUpdatedBy { get; set; }
        
    }
}