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
            var allergen = _mongoService.Allergens.Find(_ => true).ToList();
            return new PreferenceDTO
            {
                Diets = diet,
                Cuisines = cuisine,
                Allergens = allergen,
            };
        }

        public UserPreferenceDTO GetPreference(string userId)
        {
            // get all preferences from user collection with user id
            var user = _mongoService.Users.Find(_ => _.Id == userId).FirstOrDefault();
            if(user == null)
            {
                throw new Exception("User not found");
            }
            return new UserPreferenceDTO
            {
                Diets = user.Diet,
                Cuisines = user.Cuisine,
                Allergens = user.Allergen,
            };
        }

        public Task UpdatePreference(UserPreferenceDTO userPreferenceDTO)
        {
            // Update list of preferences for user id
            var filter = Builders<User>.Filter.Eq("Id", userPreferenceDTO.UserID);
            // Check if any object found from filter
            if (_mongoService.Users.Find(filter).FirstOrDefault() == null)
            {
                throw new Exception("User not found");
            }
            var update = Builders<User>.Update.Set("Diet", userPreferenceDTO.Diets)
                        .Set("Cuisine", userPreferenceDTO.Cuisines)
                        .Set("Allergen", userPreferenceDTO.Allergens);
            return _mongoService.Users.UpdateOneAsync(filter, update);
        }
    }
}