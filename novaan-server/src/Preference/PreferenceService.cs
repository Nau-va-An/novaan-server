using MongoConnector;
using MongoConnector.Models;
using MongoDB.Bson;
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

        // TODO: Check if can reduce to one request
        public AllPreferencesDTO GetAllPreferences()
        {
            var diet = _mongoService.Diets.Find(_ => true).ToList();
            var cuisine = _mongoService.Cuisines.Find(_ => true).ToList();
            var allergen = _mongoService.Allergens.Find(_ => true).ToList();
            return new AllPreferencesDTO
            {
                Diets = diet,
                Cuisines = cuisine,
                Allergens = allergen,
            };
        }

        public async Task<UserPreferenceDTO> GetUserPreferences(string userId)
        {
            var user = (await _mongoService.Users
                .FindAsync(user => user.AccountID == userId))
                .FirstOrDefault()
                ?? throw new BadHttpRequestException("User not found");

            return new UserPreferenceDTO
            {
                Diets = user.Diet,
                Cuisines = user.Cuisine,
                Allergens = user.Allergen,
            };
        }

        public async Task UpdateUserPreferences(string userId, UserPreferenceDTO userPreferenceDTO)
        {
            var foundUser = (await _mongoService.Users
                .FindAsync(user => user.AccountID == userId))
                .FirstOrDefault()
                ?? throw new BadHttpRequestException("User not found");

            var update = Builders<User>.Update.Set("Diet", userPreferenceDTO.Diets)
                        .Set("Cuisine", userPreferenceDTO.Cuisines)
                        .Set("Allergen", userPreferenceDTO.Allergens);
            await _mongoService.Users.UpdateOneAsync(user => user.Id == userId, update);
        }
    }
}