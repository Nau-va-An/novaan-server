using System.Net;
using MongoConnector;
using MongoConnector.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
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

        public async Task<UserPreferenceDTO> GetUserPreferences(string? userId)
        {
            if (userId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            var user = (await _mongoService.Users
                .FindAsync(user => user.Id == userId))
                .FirstOrDefault()
                ?? throw new BadHttpRequestException("User not found");

            return new UserPreferenceDTO
            {
                Diets = user.Diet,
                Cuisines = user.Cuisine,
                Allergens = user.Allergen,
            };
        }

        public async Task UpdateUserPreferences(string? userId, UserPreferenceDTO userPreferenceDTO)
        {
            if (userId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }

            var foundUser = (await _mongoService.Users
                .FindAsync(user => user.Id == userId))
                .FirstOrDefault()
                ?? throw new BadHttpRequestException("User not found");

            // check if diet, cuisine, allergen from userPreferenceDTO exists
            var isPreferencesExist = ValidateUserPreferences(userPreferenceDTO);
            if (!isPreferencesExist)
            {
                throw new NovaanException(ErrorCodes.PREFERENCE_NOT_FOUND, HttpStatusCode.NotFound);
            }

            var update = Builders<User>.Update
                .Set("Diet", userPreferenceDTO.Diets)
                .Set("Cuisine", userPreferenceDTO.Cuisines)
                .Set("Allergen", userPreferenceDTO.Allergens);
            await _mongoService.Users.UpdateOneAsync(user => user.Id == userId, update);
        }

        private bool ValidateUserPreferences(UserPreferenceDTO userPreferenceDTO)
        {
            var dietIds = userPreferenceDTO.Diets;
            var cuisineIds = userPreferenceDTO.Cuisines;
            var allergenIds = userPreferenceDTO.Allergens;

            var isDietExist = _mongoService.Diets.Find(d => dietIds.Contains(d.Id)).Any();
            var isCuisineExist = _mongoService.Cuisines.Find(c => cuisineIds.Contains(c.Id)).Any();
            var isAllergenExist = _mongoService.Allergens.Find(a => allergenIds.Contains(a.Id)).Any();

            return isDietExist && isCuisineExist && isAllergenExist;
        }

    }
}