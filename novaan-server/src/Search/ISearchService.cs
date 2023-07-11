using NovaanServer.src.Search.DTOs;

namespace NovaanServer.src.Search
{
    public interface ISearchService
    {
        Task<List<AdvancedSearchRes>> AdvancedSearchRecipes(string? currentUserId, AdvancedSearchReq req);
    }
}