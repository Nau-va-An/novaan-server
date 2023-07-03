using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoConnector.Models;
using NovaanServer.src.Common.Attributes;
using NovaanServer.src.Common.Utils;
using NovaanServer.src.Content.DTOs;
using NovaanServer.src.Filter;
using S3Connector;

namespace NovaanServer.src.Content
{
    [Route("api/content")]
    [ApiController]
    [Authorize]
    public class ContentController : ControllerBase
    {
        private readonly IContentService _contentService;
        private readonly S3Service _s3Service;
        public ContentController(IContentService contentService, S3Service s3Service)
        {
            _contentService = contentService;
            _s3Service = s3Service;
        }

        /// <summary>
        /// Get all recipes and culinary tips from users, where status is Approved
        /// </summary>
        /// <returns></returns>
        [HttpGet("posts")]
        public async Task<GetReelDTO> GetPersonalReels()
        {
            var userId = Request.GetUserId();
            return await _contentService.GetPersonalReel(userId);
        }

        [HttpPost("upload/tips")]
        [MultipartFormData]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadCulinaryTips()
        {
            var userId = Request.GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }
            var culinaryTips = await _contentService.ProcessMultipartRequest<CulinaryTip>(Request);
            await _contentService.UploadTips(culinaryTips, userId);
            return Ok();
        }

        [HttpPost("upload/recipe")]
        [MultipartFormData]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadRecipe()
        {
            var userId = Request.GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var recipe = await _contentService.ProcessMultipartRequest<Recipe>(Request);
            await _contentService.UploadRecipe(recipe, userId);
            return Ok();
        }

        // Validate file metadata
        [HttpPost("validate")]
        public IActionResult ValidateFileMetadata(FileInformationDTO fileMetadataDTO)
        {
            var isFileValid = _contentService.ValidateFileMetadata(fileMetadataDTO);
            if (!isFileValid)
            {
                return BadRequest();
            }
            return Ok();
        }

        // Download file
        [HttpGet("download/{id}")]
        public async Task<DownloadFileResDTO> DownloadFile(string id)
        {
            return new DownloadFileResDTO
            {
                Url = await _s3Service.GetDownloadUrlAsync(id)
            };
        }

        // User likes a post
        [HttpPost("interaction/like/{postId}")]
        public async Task<IActionResult> LikePost(string postId)
        {
            //Them param submissionType
            var userId = Request.GetUserId();
            await _contentService.LikePost(postId, userId);
            return Ok();
        }

        // Save a post 
        [HttpPost("interaction/save/{postId}")]
        public async Task<IActionResult> SavePost(string postId)
        {
            var userId = Request.GetUserId();
            await _contentService.SavePost(postId, userId);
            return Ok();
        }

        // Comment on a post
        [HttpPost("interaction/comment/{postId}")]
        public async Task<IActionResult> CommentOnPost(string postId, [FromForm] CommentDTO commentDTO)
        {
            var userId = Request.GetUserId();
            await _contentService.CommentOnPost(postId, userId, commentDTO);
            return Ok();
        }

        // Edit comment on specific post
        [HttpPut("interaction/comment/{postId}/{commentId}")]
        public async Task<IActionResult> EditCommentOnPost(string postId, string commentId, [FromForm] CommentDTO commentDTO)
        {
            var userId = Request.GetUserId();
            await _contentService.EditComment(postId,commentId ,userId, commentDTO);
            return Ok();
        }

        // Report a post
        [HttpPost("interaction/report/{postId}")]
        public async Task<IActionResult> ReportPost(string postId, [FromBody] ReportDTO reportDTO)
        {
            var userId = Request.GetUserId();
            await _contentService.ReportPost(postId, userId, reportDTO);
            return Ok();
        }

        //report a comment
        [HttpPost("interaction/report/{commentId}")]
        public async Task<IActionResult> ReportComment(string commentId, [FromBody] ReportDTO reportDTO)
        {
            var userId = Request.GetUserId();
            await _contentService.ReportComment(commentId, userId, reportDTO);
            return Ok();
        }

    }
}

