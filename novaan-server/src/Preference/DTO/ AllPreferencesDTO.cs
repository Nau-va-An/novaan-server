using MongoConnector.Models;
namespace NovaanServer.src.Preference.DTOs
{
    public class AllPreferencesDTO
    {
        public List<Diet> Diets { get; set; } = new List<Diet>();
        public List<Cuisine> Cuisines { get; set; } = new List<Cuisine>();
        public List<Allergen> Allergens { get; set; } = new List<Allergen>();
    }
}