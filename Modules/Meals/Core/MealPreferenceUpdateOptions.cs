using System;
using KidsMealApi.DataAccess.Models;

public class MealPreferenceUpdateOptions {

    public string? MealName { get; set; }
    public string? MealDescription { get; set; }
    public bool? IsSideDish { get; set; }
    public bool? IsTakeOut { get; set; }
    public List<MealType>? MealTypes { get; set; }
}