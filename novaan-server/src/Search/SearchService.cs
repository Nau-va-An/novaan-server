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
            var listBasicIngredients = JsonConvert
                .DeserializeObject<List<string>>(basicIngredients)
                ?? new List<string>();

            //Remove the basic ingredients ignore case from the input list, resulting in a list of ingredients without the basic ones.
            var ingredients = req.Ingredients
                .Where(i => !listBasicIngredients.Contains(i, StringComparer.OrdinalIgnoreCase))
                .ToList();

            Dictionary<string, List<int>> recipeMappings = new();

            int i = 0;

            while (true)
            {
                if (i >= ingredients.Count)
                {
                    i = 0;
                    continue;
                }

                if (recipeMappings.Count >= ingredients.Count)
                {
                    break;
                }

                var ingredient = ingredients[i];
                var ingredientToRecipes = (await _mongoDBService.IngredientToRecipes
                    .FindAsync(i => i.Ingredient == ingredient))
                    .FirstOrDefault();

                if (ingredientToRecipes == null){
                    continue;
                }

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

                i++;
            }

            // After the loop ends, extract the recipe arrays from the dictionary.
            var recipeIds = recipeMappings
                .Where(r => r.Value.Count == ingredients.Count)
                .Select(r => r.Key)
                .ToList()
                ?? throw new NovaanException(ErrorCodes.SEARCH_INGREDIENTS_NOT_FOUND, HttpStatusCode.NotFound);

            //Return the extracted arrays, which represent the recipes that can be made using the input ingredients.
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