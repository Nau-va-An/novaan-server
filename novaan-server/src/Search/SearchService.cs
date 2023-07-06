using MongoConnector;
using Newtonsoft.Json;
using NovaanServer.src.Search.DTOs;
using MongoDB.Driver;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using System.Net;

namespace NovaanServer.src.Search
{
    public class SearchService : ISearchService
    {
        private readonly MongoDBService _mongoDBService;

        public SearchService(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        public async Task<List<GetRecipesByIngredientsRes>> AdvancedSearchRecipes(string? currentUserId, AdvancedSearchRecipesReq req)
        {
            // read basic ingredients from the json file in same folder
            var basicIngredients = File.ReadAllText("BasicIngredients.json");
            var listBasicIngredients = JsonConvert.DeserializeObject<List<string>>(basicIngredients) ?? new List<string>();

            // Remove the basic ingredients, ignoring case, from the input list
            var ingredients = req.Ingredients
                .Where(i => !listBasicIngredients.Contains(i, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var recipeMappings = new Dictionary<string, List<int>>();

            // Initialize counters and variables
            int i = 0;
            int endedArray = 0;

            while (endedArray < ingredients.Count)
            {
                if (i >= ingredients.Count)
                {
                    i = 0;
                    endedArray++;
                    continue;
                }

                var ingredient = ingredients[i];
                var ingredientToRecipes =  (await _mongoDBService.IngredientToRecipes
                    .Find(i => i.Ingredient.Equals(ingredient, StringComparison.OrdinalIgnoreCase))
                    .ToListAsync())
                    .FirstOrDefault();

                // Iterate over the recipe IDs associated with the current ingredient and update the recipeMappings dictionary accordingly. 
                // If the recipe ID already exists in the dictionary, append the current ingredient index to the corresponding list.
                // Otherwise, create a new entry in the dictionary with the recipe ID as the key and a list containing the current ingredient index.
                if (ingredientToRecipes != null)
                {
                    foreach (var recipeId in ingredientToRecipes.RecipeIds)
                    {
                        if (recipeMappings.ContainsKey(recipeId))
                        {
                            recipeMappings[recipeId].Add(i);
                        }
                        else
                        {
                            recipeMappings.Add(recipeId, new List<int> { i });
                        }
                    }
                }

                i++;
            }

            // Extract the recipe IDs
            var recipeIds = recipeMappings
                .Where(r => r.Value.Count == ingredients.Count)
                .Select(r => r.Key)
                .ToList();

            // Return the recipes that can be made using the input ingredients
            return await _mongoDBService.Recipes
                .Find(r => recipeIds.Contains(r.Id))
                .Project(r => new GetRecipesByIngredientsRes
                {
                    Id = r.Id,
                    Title = r.Title,
                    Thumbnails = r.Video,
                    CookTime = r.CookTime,
                })
                .ToListAsync();

        }
    }
}