using MongoConnector;
using MongoDB.Driver;
using NovaanServer.src.Preference.DTOs;

namespace NovaanServer.src.Preference
{
    public class PreferenceService : IPreferenceService
    {
        private readonly MongoDBService _mongoService;

        public PreferenceService(MongoDBService mongoService)
        {
            _mongoService = mongoService;
        }

        public List<PreferenceDTO> GetAllPreference()
        {
            var preference = new List<PreferenceDTO>();
            var diet = _mongoService.Diets.Find(_ => true).ToList();
            var cuisine = _mongoService.Cuisines.Find(_ => true).ToList();
            var mealType = _mongoService.MealTypes.Find(_ => true).ToList();
            preference.Add(new PreferenceDTO()
            {
                Diets = diet,
                Cuisines = cuisine,
                MealTypes = mealType
            });
            return preference;
        }
    }

}