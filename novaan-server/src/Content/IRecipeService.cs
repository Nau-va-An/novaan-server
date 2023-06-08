using NovaanServer.src.Content.DTOs;

namespace NovaanServer.src.Recipe
{
    public interface IRecipeService
    {
        public Task<bool> UploadRecipe(RecipeDTO recipeDTOs);

    }
}
