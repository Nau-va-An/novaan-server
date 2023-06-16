using MongoConnector.Models;
namespace NovaanServer.src.Preference.DTOs
{
    public class PreferenceDTO
    {
        public List<Diet> Diets { get; set; }
        public List<Cuisine> Cuisines { get; set; }
        public List<Allergen> Allergens { get; set; }
    }
}