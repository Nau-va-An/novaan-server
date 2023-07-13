using MongoConnector;
using Newtonsoft.Json;
using NovaanServer.src.Search.DTOs;
using MongoDB.Driver;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using System.Net;
using MongoConnector.Models;
using mongo_connector.Models;

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

        public async Task<List<AdvancedSearchRes>> AdvancedSearchRecipes(
            string? currentUserId, AdvancedSearchReq req
        )
        {
            // Remove the basic ingredients, ignoring case, from the input list
            var ingredients = req.Ingredients;

            // Counter to keep track how many ingredients remove during filter
            int basicIngredientNum = 0;
            ingredients = ingredients
                // Format current ingredients
                .Select(ingredient => ingredient.Trim())
                // Filter out basic ingredients
                .Where(ingredient =>
                {
                    var isBasicIngredient = _basicIngredients
                        .Contains(ingredient, StringComparer.OrdinalIgnoreCase);
                    if (isBasicIngredient)
                    {
                        basicIngredientNum++;
                    }
                    return !isBasicIngredient;
                })
                .ToList();

            var ingredientFilter = Builders<IngredientToRecipes>.Filter.In(itr => itr.Ingredient, ingredients);
            var ingredientRecipeMap = (await _mongoDBService.IngredientToRecipes
                .FindAsync(ingredientFilter))
                .ToList();

            // Return empty result when query ingredient from database return empty
            if (ingredientRecipeMap.Count == 0)
            {
                return new();
            }

            var recipeIngredientMap = new Dictionary<string, int>();

            int mostRelevance = 0; // Count the number of matched ingredients
            string mostRelevanceKey = string.Empty; // Key of the most relevance recipe

            // O(m*n) where
            // m is the number of input ingredients
            // n is the average number of recipes each ingredient belong to
            foreach (var item in ingredientRecipeMap)
            {
                foreach (var recipeId in item.RecipeIds)
                {
                    if (recipeIngredientMap.ContainsKey(recipeId))
                    {
                        recipeIngredientMap[recipeId]++;
                        if (recipeIngredientMap[recipeId] > mostRelevance)
                        {
                            mostRelevance = recipeIngredientMap[recipeId];
                            mostRelevanceKey = recipeId;
                        }
                    }
                    else
                    {
                        recipeIngredientMap[recipeId] = 1;
                        if (mostRelevance == 0)
                        {
                            mostRelevance = 1;
                            mostRelevanceKey = recipeId;
                        }
                    }
                }
            }

            // Return empty when the dictionary contain no entry
            var recipeIds = recipeIngredientMap.Keys.ToList();
            if (recipeIds.Count == 0)
            {
                return new();
            }

            // where r => r.Ingredients.Count == recipeIngredientMap[r.Id] and r.Id in Recipe collection
            var recipeFilter = Builders<Recipe>.Filter.In(r => r.Id, recipeIds) &
                Builders<Recipe>.Filter.SizeLte(r => r.Ingredients, mostRelevance + basicIngredientNum);

            //  get all recipe that had id in recipe collection with same Ingredients.Count and join with user collection to get author name 
            var matchRecipes = (await _mongoDBService.Recipes
                .Aggregate()
                .Match(recipeFilter)
                // get user name only from user collection
                .Lookup(
                    foreignCollection: _mongoDBService.Users,
                    localField: r => r.CreatorId,
                    foreignField: u => u.Id,
                    @as: (RecipeWithUserInfoDTO r) => r.Author
                )
                .ToListAsync())
                .Where(r => r.Ingredients.Count == recipeIngredientMap[r.Id] + basicIngredientNum)
                .ToList();

            matchRecipes.Sort((a, b) => recipeIngredientMap[b.Id].CompareTo(recipeIngredientMap[a.Id]));
            
            var result = matchRecipes.Select(mr => new AdvancedSearchRes()
            {
                Id = mr.Id,
                Title = mr.Title,
                Thumbnails = mr.Video,
                CookTime = mr.CookTime,
                // Author come back as an array but should be only one author for each recipe
                AuthorId = mr.Author.Select(a => a.Id).FirstOrDefault() ?? "",
                AuthorName = mr.Author.Select(a => a.DisplayName).FirstOrDefault() ?? "",
                Saved = mr.Author.Any(a => a.SavedPosts.Any(sp => sp.PostId == mr.Id))
            }).ToList();

            return result;
        }
    }
}