using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MongoConnector.Models;
using NovaanServer.src.Content.DTOs;
using NovaanServer.src.Filter;
using S3Connector;

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

        [HttpPost("upload/culinary-tips")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadCulinaryTips()
        {
            var culinaryTips = await _contentService.ProcessMultipartRequest(Request);
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
    }
}

