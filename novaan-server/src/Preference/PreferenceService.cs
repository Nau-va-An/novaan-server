using MongoConnector;
using MongoConnector.Models;
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

        public UserPreferenceDTO GetPreference(string userId)
        {
            // get all preferences from user collection with user id
            var user = _mongoService.Users.Find(_ => _.Id == userId).FirstOrDefault();
            return new UserPreferenceDTO
            {
                Diets = user.DietID,
                Cuisines = user.CuisineID,
                MealTypes = user.MealTypeID,
            };
        }

        public Task UpdatePreference(string userId, List<string> dietId, List<string> cuisineId, List<string> mealTypeId)
        {
            // Update list of preferences for user id
            var filter = Builders<User>.Filter.Eq("Id", userId);
            var update = Builders<User>.Update.Set("DietID", dietId)
                .Set("CuisineID", cuisineId)
                .Set("MealTypeID", mealTypeId);
            return _mongoService.Users.UpdateOneAsync(filter, update);
        }
    }

}