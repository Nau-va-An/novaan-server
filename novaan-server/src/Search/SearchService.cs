using MongoConnector;
using Newtonsoft.Json;
using NovaanServer.src.Search.DTOs;
using MongoDB.Driver;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using System.Net;
using MongoConnector.Models;

namespace NovaanServer.src.Search
{
    public class SearchService : ISearchService
    {
        private readonly MongoDBService _mongoDBService;
        private static readonly List<string> _basicIngredients =
            JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("BasicIngredients.json")) ?? new();

        public SearchService(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        public async Task<List<GetRecipesByIngredientsRes>> AdvancedSearchRecipes(string? currentUserId, AdvancedSearchRecipesReq req)
        {
            // Remove the basic ingredients, ignoring case, from the input list
            var ingredients = req.Ingredients;

            ingredients = ingredients
                // Format current ingredients
                .Select(ingredient => ingredient.Trim().ToLower())
                // Filter out basic ingredients
                .Where(ingredient => !_basicIngredients.Contains(ingredient))
                .ToList();

            var ingredientRecipeMap = (await _mongoDBService.IngredientToRecipes
                .FindAsync(i => ingredients.Contains(i.Ingredient)))
                .ToList();

            var recipeIngredientMap = new Dictionary<string, int>();

            // Initialize counters and variables
            int i = 0;
            int endedArray = 0;
            int mostRelevance = 0; // Count the number of matched ingredients
            string mostRelevanceKey = string.Empty; // Key of the most relevance recipe

            while (endedArray < ingredients.Count)
            {
                endedArray = 0;
                foreach (var item in ingredientRecipeMap)
                {
                    if (i >= item.RecipeIds.Count)
                    {
                        endedArray++;
                        continue;
                    }

                    var currentRecipe = item.RecipeIds[i];
                    if (currentRecipe == null)
                    {
                        continue;
                    }

                    if (recipeIngredientMap.ContainsKey(currentRecipe))
                    {
                        recipeIngredientMap[currentRecipe]++;
                        if (recipeIngredientMap[currentRecipe] > mostRelevance)
                        {
                            mostRelevance = recipeIngredientMap[currentRecipe];
                            mostRelevanceKey = currentRecipe;
                        }
                    }
                    else
                    {
                        recipeIngredientMap[currentRecipe] = 1;
                        if (mostRelevance == 0)
                        {
                            mostRelevance = 1;
                            mostRelevanceKey = currentRecipe;
                        }
                    }
                }

                i++;
            }

            var recipeIds = recipeIngredientMap.Keys.ToList();
            var recipeFilter = Builders<Recipe>.Filter.In(r => r.Id, recipeIds);
            var matchRecipes = (await _mongoDBService.Recipes
                .Find(recipeFilter)
                .ToListAsync())
                .Where(r => r.Ingredients.Count == recipeIngredientMap[r.Id])
                .ToList();

            matchRecipes.Sort((a, b) => recipeIngredientMap[b.Id].CompareTo(recipeIngredientMap[a.Id]));

            var result = matchRecipes.Select(mr => new GetRecipesByIngredientsRes()
            {
                Id = mr.Id,
                Title = mr.Title,
                Thumbnails = mr.Video,
                CookTime = mr.CookTime,
            }).ToList();

            return result;
        }
    }
}