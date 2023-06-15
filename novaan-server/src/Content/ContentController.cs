using Microsoft.AspNetCore.Mvc;
using MongoConnector.Models;
using NovaanServer.src.Content.DTOs;
using NovaanServer.src.Filter;

namespace NovaanServer.src.Content
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly IContentService _contentService;
        public ContentController(IContentService contentService)
        {
            _contentService = contentService;
        }

        /// <summary>
        /// Get all recipes and culinary tips from users, where status is Approved
        /// </summary>
        /// <returns></returns>
        [HttpGet("posts")]
        public async Task<IActionResult> GetPosts()
        {
            var posts = _contentService.GetPosts();
            return Ok(posts);
        }

        [HttpPost("upload/culinary-tips")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadCulinaryTips()
        {
            var culinaryTips = await _contentService.ProcessMultipartRequest<CulinaryTip>(Request);
            // Add to database
            await _contentService.AddCulinaryTips(culinaryTips);
            return Ok();
        }

        // Validate file metadata
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateFileMetadata([FromBody] FileInformationDTO fileMetadataDTO)
        {
            await _contentService.ValidateFileMetadata(fileMetadataDTO);
            return Ok();
        }

        [HttpPost("upload/recipe")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadRecipe()
        {
            Recipe recipe = await _contentService.ProcessMultipartRequest<Recipe>(Request);
            await _contentService.UploadRecipe(recipe);
            return Ok();

        }
    }
}

