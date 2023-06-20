using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoConnector.Models;
using NovaanServer.src.Common.Attributes;
using NovaanServer.src.Content.DTOs;
using NovaanServer.src.Filter;
using Utils.Json;

namespace NovaanServer.src.Content
{
    [Route("api/content")]
    [ApiController]
    //[Authorize]
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
        public PostDTO GetPosts()
        {
            return _contentService.GetPosts();
        }

        [HttpPost("upload/tips")]
        [MultipartFormData]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadCulinaryTips()
        {
            var culinaryTips = await _contentService.ProcessMultipartRequest<CulinaryTip>(Request);
            await _contentService.UploadTips(culinaryTips);
            return Ok();
        }

        [HttpPost("upload/recipe")]
        [MultipartFormData]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadRecipe()
        {
            var recipe = await _contentService.ProcessMultipartRequest<Recipe>(Request);
            await _contentService.UploadRecipe(recipe);
            return Ok();
        }

        // Validate file metadata
        [HttpPost("validate")]
        public IActionResult ValidateFileMetadata([FromBody] FileInformationDTO fileMetadataDTO)
        {
            var isFileValid = _contentService.ValidateFileMetadata(fileMetadataDTO);
            if (!isFileValid)
            {
                return BadRequest();
            }
            return Ok();
        }

        
    }
}

