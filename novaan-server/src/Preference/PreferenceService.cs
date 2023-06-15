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

        public PreferenceDTO GetAllPreferences()
        {
            var diet = _mongoService.Diets.Find(_ => true).ToList();
            var cuisine = _mongoService.Cuisines.Find(_ => true).ToList();
            var mealType = _mongoService.MealTypes.Find(_ => true).ToList();
            return new PreferenceDTO
            {
                Diets = diet,
                Cuisines = cuisine,
                MealTypes = mealType
            };
        }

        public Task UpdatePreference()
        {
            throw new NotImplementedException();
        }
    }

}