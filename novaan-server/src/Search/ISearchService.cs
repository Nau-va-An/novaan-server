using NovaanServer.src.Search.DTOs;

namespace NovaanServer.src.Search
{
    public interface ISearchService
    {
        Task<List<GetRecipesByIngredientsRes>> AdvancedSearchRecipes(string? currentUserId, AdvancedSearchRecipesReq req);
    }
}