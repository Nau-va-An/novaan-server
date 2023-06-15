using System;
namespace NovaanServer.src.Preference.DTOs
{
    public class UserPreferenceDTO
    {
        public string? UserID { get; set; }
        public List<string> Diets { get; set; }
        public List<string> Cuisines { get; set; }
        public List<string> MealTypes { get; set; }
    }
}

