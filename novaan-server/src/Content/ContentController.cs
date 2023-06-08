using Microsoft.AspNetCore.Mvc;
using NovaanServer.src.Content.DTOs;
using NovaanServer.src.Image.DTOs;

namespace NovaanServer.src.Content
{
    [Route("api/content")]
    [ApiController]
    public class ContentController: Controller
    {
        private readonly IRecipeService _recipeService;
        public ContentController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        [HttpPost("upload/recipe")]
        public async Task<IActionResult> UploadRecipe([FromForm] RecipeDTO recipeDTO)
        {

            await _recipeService.UploadRecipe(recipeDTO);
            return Ok();

        }
    }
}
