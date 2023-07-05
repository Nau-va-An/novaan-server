using Microsoft.AspNetCore.Mvc;
using NovaanServer.src.Common.Utils;
using NovaanServer.src.Search.DTOs;

namespace NovaanServer.src.Search
{
    [Route("api/search")]
    [ApiController]
    // [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("advanced/recipes")]
        public async Task<List<GetRecipesByIngredientsRes>> AdvancedSearchRecipes([FromQuery] AdvancedSearchRecipesReq req)
        {
            var currentUserId = Request.GetUserId();
            return await _searchService.AdvancedSearchRecipes(currentUserId, req);
        }
    }
}