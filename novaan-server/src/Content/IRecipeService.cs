using Microsoft.AspNetCore.Mvc;
using NovaanServer.src.Content.DTOs;

namespace NovaanServer.src.Content
{

    public interface IRecipeService
    {
        public Task<bool> UploadRecipe(RecipeDTO recipeDTOs);

    }
}
