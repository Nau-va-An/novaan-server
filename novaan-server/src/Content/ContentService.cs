using System.ComponentModel.DataAnnotations;
using System.Net;
using Amazon.S3;
using FileSignatures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MongoConnector;
using MongoConnector.Enums;
using MongoConnector.Models;
using MongoDB.Driver;
using NovaanServer.src.Content.DTOs;
using NovaanServer.src.Content.FormHandler;
using NovaanServer.src.Content.Settings;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using S3Connector;
using Utils.UtilClass;

namespace NovaanServer.src.Content
{
    public class ContentService : IContentService
    {
        private readonly MongoDBService _mongoService;
        private readonly S3Service _s3Service;

        private readonly DelayExecutioner _delayExecutioner = new();

        public ContentService(MongoDBService mongoDBService, S3Service s3Service)
        {
            _mongoService = mongoDBService;
            _s3Service = s3Service;
        }

        // Process multipart request
        public async Task<T> ProcessMultipartRequest<T>(HttpRequest request)
        {
            var boundary = MultipartHandler
                .GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType));
            var reader = new MultipartReader(boundary, request.Body);
            var section = await reader.ReadNextSectionAsync();

            var obj = Activator.CreateInstance<T>() ??
                throw new NovaanException(ErrorCodes.SERVER_UNAVAILABLE);
            var properties = typeof(T).GetProperties();

            while (section != null)
            {
                var disposition = section.GetContentDispositionHeader();
                if (disposition == null || disposition.Name == null)
                {
                    throw new NovaanException(
                        ErrorCodes.CONTENT_DISPOSITION_INVALID,
                        HttpStatusCode.BadRequest
                    );
                }

                if (MultipartHandler.HasFormDataContentDisposition(disposition))
                {
                    // Handle form data
                    var formSection = section.AsFormDataSection() ??
                        throw new NovaanException(
                            ErrorCodes.CONTENT_DISPOSITION_INVALID,
                            HttpStatusCode.BadRequest
                        );

                    var fieldName = formSection.Name;
                    var value = await formSection.GetValueAsync();
                    var property = properties.FirstOrDefault(p => p.Name == fieldName);

                    /*
                     * Indicate that this is an list-related value with following signature:
                     * PropertyName_Index_NestedProperty
                    */
                    if (property == null)
                    {
                        HandleListSection(obj, fieldName, value);
                    }
                    else
                    {
                        // Throw error if property contain video or contain image
                        if (property.Name.ToLower().Contains("video") || property.Name.ToLower().Contains("image"))
                        {
                            throw new NovaanException(
                                ErrorCodes.CONTENT_FIELD_INVALID,
                                HttpStatusCode.BadRequest
                            );
                        }
                        // Normal signature
                        CustomMapper.MappingObjectData(obj, property, value);
                    }
                }

                if (MultipartHandler.HasFileContentDisposition(disposition))
                {
                    // Handle file data
                    var fileSection = section.AsFileSection() ??
                        throw new NovaanException(
                            ErrorCodes.CONTENT_DISPOSITION_INVALID,
                            HttpStatusCode.BadRequest
                        );

                    var fileStream = await ProcessStreamContent(fileSection);

                    var fileId = System.Guid.NewGuid().ToString() +
                            Path.GetExtension(fileSection.FileName);
                    _delayExecutioner.AppendAction(
                        async () => await _s3Service.UploadFileAsync(fileStream, fileId)
                    );

                    var fieldName = fileSection.Name;
                    var property = properties.FirstOrDefault(p => p.Name == fieldName);

                    /*
                     * Indicate that this is an list-related value with following signature:
                     * PropertyName_Index_NestedProperty
                    */
                    if (property == null)
                    {
                        HandleListSection(obj, fieldName, fileId);
                    }
                    else
                    {
                        // Normal signature for Video
                        CustomMapper.MappingObjectData(obj, property, fileId);
                    }
                    // Will close stream when S3 finish uploading
                }

                section = await reader.ReadNextSectionAsync();
            }
            return obj;
        }

        public bool ValidateFileMetadata(FileInformationDTO fileMetadataDTO)
        {
            // Image requirements check
            if (ContentSettings.PermittedImgExtension.Contains(fileMetadataDTO.FileExtension))
            {
                if (fileMetadataDTO.Width < 640 || fileMetadataDTO.Height < 480)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_IMG_RESO_INVALID, HttpStatusCode.BadRequest);
                }

                if (fileMetadataDTO.FileSize > ContentSettings.ImageSizeLimit)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_IMG_SIZE_INVALID, HttpStatusCode.BadRequest);
                }

                return true;
            }

            // Video requirements check
            if (ContentSettings.PermittedVidExtension.Contains(fileMetadataDTO.FileExtension))
            {
                if (fileMetadataDTO.Width / fileMetadataDTO.Height != 16 / 9)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_VID_RESO_INVALID, HttpStatusCode.BadRequest);
                }

                if (fileMetadataDTO.VideoDuration > TimeSpan.FromMinutes(2))
                {
                    throw new NovaanException(ErrorCodes.CONTENT_VID_LEN_INVALID, HttpStatusCode.BadRequest);
                }

                if (fileMetadataDTO.FileSize > ContentSettings.VideoSizeLimit)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_VID_SIZE_INVALID, HttpStatusCode.BadRequest);
                }

                return true;
            }

            throw new NovaanException(ErrorCodes.CONTENT_EXT_INVALID, HttpStatusCode.BadRequest);
        }

        public async Task UploadRecipe(Recipe recipe, string creatorId)
        {
            if (creatorId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Validate recipe object against its model annotation
            var context = new ValidationContext(recipe, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(recipe, context, validationResults, true);
            if (!isValid)
            {
                throw new NovaanException(
                    ErrorCodes.CONTENT_FIELD_INVALID,
                    HttpStatusCode.BadRequest,
                    validationResults.FirstOrDefault().ErrorMessage ?? string.Empty
                );
            }
            if (recipe.Ingredients.Count == 0)
            {
                throw new NovaanException(ErrorCodes.FIELD_REQUIRED, HttpStatusCode.BadRequest);
            }
            if (recipe.Instructions.Count == 0)
            {
                throw new NovaanException(ErrorCodes.FIELD_REQUIRED, HttpStatusCode.BadRequest);
            }
            if (recipe.CookTime.CompareTo(ContentSettings.COOK_MAX) > 0)
            {
                throw new NovaanException(ErrorCodes.CONTENT_COOK_TIME_TOO_LONG, HttpStatusCode.BadRequest);
            }
            if (recipe.PrepTime.CompareTo(ContentSettings.PREP_MAX) > 0)
            {
                throw new NovaanException(ErrorCodes.CONTENT_PREP_TIME_TOO_LONG, HttpStatusCode.BadRequest);
            }

            // Try to push to S3 here
            try
            {
                await _delayExecutioner.Execute();
            }
            catch (AmazonS3Exception e)
            {
                throw new NovaanException(
                    ErrorCodes.S3_UNAVAILABLE,
                    HttpStatusCode.InternalServerError,
                    e.Message
                );
            }
            _delayExecutioner.Cleanup();

            // Set remaining missing fields
            recipe.CreatorId = creatorId;
            recipe.Status = Status.Pending;
            try
            {
                await _mongoService.Recipes.InsertOneAsync(recipe);
            }
            catch
            {
                throw new NovaanException(ErrorCodes.DATABASE_UNAVAILABLE);
            }
        }

        public async Task UploadTips(CulinaryTip culinaryTips, string creatorId)
        {
            // Validate tips and tricks field here
            var context = new ValidationContext(culinaryTips, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(culinaryTips, context, validationResults, true);
            if (!isValid)
            {
                throw new NovaanException(
                    ErrorCodes.CONTENT_FIELD_INVALID,
                    HttpStatusCode.BadRequest,
                    validationResults.FirstOrDefault().ErrorMessage ?? string.Empty
                );
            }

            // Try to push to S3 here
            try
            {
                await _delayExecutioner.Execute();
            }
            catch (AmazonS3Exception e)
            {
                throw new NovaanException(
                    ErrorCodes.S3_UNAVAILABLE,
                    HttpStatusCode.InternalServerError,
                    e.Message
                );
            }
            _delayExecutioner.Cleanup();

            // Set remaning missing field
            culinaryTips.CreatorId = creatorId;
            culinaryTips.Status = Status.Pending;
            try
            {
                await _mongoService.CulinaryTips.InsertOneAsync(culinaryTips);
            }
            catch
            {
                throw new NovaanException(ErrorCodes.DATABASE_UNAVAILABLE);
            }
        }

        public PostDTO GetPosts()
        {
            try
            {
                var recipes = _mongoService.Recipes.Find(r => r.Status == Status.Approved).ToList();
                var culinaryTips = _mongoService.CulinaryTips.Find(c => c.Status == Status.Approved).ToList();
                return new PostDTO
                {
                    RecipeList = recipes,
                    CulinaryTips = culinaryTips
                };
            }
            catch
            {
                throw new NovaanException(ErrorCodes.DATABASE_UNAVAILABLE);
            }
        }

        private static void HandleListSection<T>(T? obj, string fieldName, string value)
        {
            var splitValues = fieldName.Split("_");
            if (splitValues.Length != 3)
            {
                throw new NovaanException(
                    ErrorCodes.CONTENT_FIELD_INVALID,
                    HttpStatusCode.BadRequest
                );
            }

            // splitValues[0] should always be "Instruction"
            var properties = typeof(T).GetProperties();
            var property = properties.FirstOrDefault(p => p.Name == splitValues[0]) ??
                throw new NovaanException(ErrorCodes.SERVER_UNAVAILABLE);


            bool canParse = int.TryParse(splitValues[1], out var nestedFieldKey);
            if (!canParse)
            {
                throw new NovaanException(ErrorCodes.CONTENT_FIELD_INVALID, HttpStatusCode.BadRequest);
            }
            var nestedFieldName = splitValues[2];

            CustomMapper.MappingObjectData(obj, property, value, nestedFieldName, nestedFieldKey);
        }

        private async Task<MemoryStream> ProcessStreamContent(FileMultipartSection section)
        {
            var fileStream = section.FileStream ??
                throw new NovaanException(ErrorCodes.SERVER_UNAVAILABLE);
            var memStream = new MemoryStream();
            await fileStream.CopyToAsync(memStream);

            // Check if the file is empty
            if (fileStream == null || memStream == null || memStream.Length == 0)
            {
                throw new Exception("File is empty");
            }

            var extension = Path.GetExtension(section.FileName);
            if (extension == null)
            {
                throw new NovaanException(ErrorCodes.CONTENT_EXT_INVALID, HttpStatusCode.BadRequest);
            }

            var isImage = ContentSettings.PermittedImgExtension.Contains(extension);
            var isVideo = !isImage && ContentSettings.PermittedVidExtension.Contains(extension);
            if (!isImage && !isVideo)
            {
                throw new NovaanException(ErrorCodes.CONTENT_EXT_INVALID, HttpStatusCode.BadRequest);
            }

            if (isImage)
            {
                if (fileStream.Length > ContentSettings.ImageSizeLimit)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_IMG_SIZE_INVALID, HttpStatusCode.BadRequest);
                }
                ValidateFileExtensionAndSignature(
                    extension,
                    memStream,
                    ContentSettings.PermittedImgExtension
                );
            }

            if (isVideo)
            {
                if (fileStream.Length > ContentSettings.VideoSizeLimit)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_VID_SIZE_INVALID, HttpStatusCode.BadRequest);
                }
                ValidateFileExtensionAndSignature(
                    extension,
                    memStream,
                    ContentSettings.PermittedVidExtension
                );
            }

            return memStream;
        }

        private static void ValidateFileExtensionAndSignature(
            string extension,
            MemoryStream data,
            string[] permittedExtensions
        )
        {
            var inspector = new FileFormatInspector();
            var fileFormat = inspector.DetermineFileFormat(data);

            // JPG and JPEG is the same
            var contentExt = "." + fileFormat?.Extension;
            if (contentExt == ".jpg" && extension == ".jpeg")
            {
                contentExt = ".jpeg";
            }

            if (
                fileFormat == null ||
                extension != contentExt
            )
            {
                throw new NovaanException(ErrorCodes.CONTENT_EXT_INVALID, HttpStatusCode.BadRequest);
            }
        }

        public async Task LikePost(string postId, string? userId)
        {
            if (userId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Find recipe or tip that has the postId
            var recipe = (await _mongoService.Recipes
                .FindAsync(r => r.Id == postId))
                .FirstOrDefault();
            var tip = (await _mongoService.CulinaryTips
                .FindAsync(t => t.Id == postId))
                .FirstOrDefault();

            if (recipe == null && tip == null)
            {
                throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Check if user has liked this post before
            var hasLikedPost = (await _mongoService.Likes
                .FindAsync(l => l.PostId == postId && l.UserId == userId && l.postType == (recipe != null ? SubmissionType.Recipe : SubmissionType.CulinaryTip)))
                .FirstOrDefault() != null;

            // Handle like/unlike post
            if (hasLikedPost)
            {
                // Unlike post
                await _mongoService.Likes
                    .DeleteOneAsync(l => l.PostId == postId && l.UserId == userId && l.postType == (recipe != null ? SubmissionType.Recipe : SubmissionType.CulinaryTip));
            }
            else
            {
                // Like post
                await _mongoService.Likes
                    .InsertOneAsync(new Likes
                    {
                        UserId = userId,
                        PostId = postId,
                        postType = recipe != null ? SubmissionType.Recipe : SubmissionType.CulinaryTip
                    });
            }

            // Update likeCount
            if (recipe != null)
            {
                await _mongoService.Recipes
                       .UpdateOneAsync(r => r.Id == postId, Builders<Recipe>.Update
                       .Inc(r => r.LikesCount, hasLikedPost ? -1 : 1));
            }
            else
            {
                await _mongoService.CulinaryTips
                    .UpdateOneAsync(t => t.Id == postId, Builders<CulinaryTip>.Update
                    .Inc(t => t.LikesCount, hasLikedPost ? -1 : 1));
            }
        }

        public async Task SavePost(string postId, string? userId)
        {
            if (userId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Find recipe or tip that has the postId
            var recipe = (await _mongoService.Recipes
                .FindAsync(r => r.Id == postId))
                .FirstOrDefault();
            var tip = (await _mongoService.CulinaryTips
                .FindAsync(t => t.Id == postId))
                .FirstOrDefault();

            if (recipe == null && tip == null)
            {
                throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Check if user has saved this post before in savedPost list of user collection
            var hasSavedPost = (await _mongoService.Users
                .FindAsync(u => u.Id == userId && u.SavedPost.Any(p => p.PostId == postId)))
                .FirstOrDefault() != null;

            if (!hasSavedPost)
            {
                // Save post
                await _mongoService.Users
                    .UpdateOneAsync(u => u.Id == userId, Builders<User>.Update
                    .Push(u => u.SavedPost, new SavedPost
                    {
                        PostId = postId,
                        PostType = recipe != null ? SubmissionType.Recipe : SubmissionType.CulinaryTip
                    }));
            }
            else
            {
                throw new NovaanException(ErrorCodes.CONTENT_ALREADY_SAVED, HttpStatusCode.BadRequest);
            }
        }

        public async Task CommentOnPost(string postId, string? userId, CommentDTO commentDTO)
        {
            if (userId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Find recipe or tip that has the postId
            var recipe = (await _mongoService.Recipes
                .FindAsync(r => r.Id == postId))
                .FirstOrDefault();

            var tip = (await _mongoService.CulinaryTips
                .FindAsync(t => t.Id == postId))
                .FirstOrDefault();

            if (recipe == null && tip == null)
            {
                throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Check if user has commented on this post before
            var hasCommented = (await _mongoService.Comments
                .FindAsync(c => c.PostId == postId && c.UserId == userId && c.postType == (recipe != null ? SubmissionType.Recipe : SubmissionType.CulinaryTip)))
                .FirstOrDefault() != null;

            if (hasCommented)
            {
                throw new NovaanException(ErrorCodes.CONTENT_ALREADY_COMMENTED, HttpStatusCode.BadRequest);
            }

            string imageId = "";
            if (commentDTO.Image != null)
            { // Upload image to S3
                imageId = System.Guid.NewGuid().ToString() +
                               Path.GetExtension(commentDTO.Image.FileName);
                await _s3Service.UploadFileAsync(commentDTO.Image.OpenReadStream(), imageId);
            }

            // Comment on post
            await _mongoService.Comments
                .InsertOneAsync(new Comments
                {
                    UserId = userId,
                    PostId = postId,
                    postType = recipe != null ? SubmissionType.Recipe : SubmissionType.CulinaryTip,
                    Comment = commentDTO.Comment,
                    CreatedAt = DateTime.Now,
                    Image = imageId,
                    Rating = commentDTO.Rating
                });

            // Update ratingCount and rating average of recipe or tip
            if (recipe != null)
            {
                await _mongoService.Recipes
                    .UpdateOneAsync(r => r.Id == postId, Builders<Recipe>.Update
                    .Inc(r => r.RatingsCount, 1)
                    .Set(r => r.AverageRating, CalculateRatingAverage(recipe.RatingsCount, recipe.AverageRating, null, commentDTO.Rating)));
            }
            else
            {
                await _mongoService.CulinaryTips
                    .UpdateOneAsync(t => t.Id == postId, Builders<CulinaryTip>.Update
                    .Inc(t => t.RatingsCount, 1)
                    .Set(t => t.AverageRating, CalculateRatingAverage(tip.RatingsCount, tip.AverageRating, null, commentDTO.Rating)));
            }
        }

        public async Task EditComment(string postId, string commentId, string? userId, CommentDTO commentDTO)
        {
            // Get comment of user on this post
            var comment = (await _mongoService.Comments
                .FindAsync(c => c.Id == commentId && c.UserId == userId && c.PostId == postId))
                .FirstOrDefault();

            if (comment == null)
            {
                throw new NovaanException(ErrorCodes.COMMENT_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            string imageId = "";
            if (commentDTO.Image != null)
            {
                // upload image to S3
                imageId = System.Guid.NewGuid().ToString() +
                                Path.GetExtension(commentDTO.Image.FileName);
                await _s3Service.UploadFileAsync(commentDTO.Image.OpenReadStream(), imageId);
            }

            // Update rating average of post with new rating from user and ratingaverage of post before edit
            if (commentDTO.Rating != comment.Rating)
            {
                var recipe = (await _mongoService.Recipes
                    .FindAsync(r => r.Id == postId))
                    .FirstOrDefault();

                var tip = (await _mongoService.CulinaryTips
                    .FindAsync(t => t.Id == postId))
                    .FirstOrDefault();

                if (recipe == null && tip == null)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);
                }

                if (recipe != null)
                {
                    await _mongoService.Recipes
                        .UpdateOneAsync(r => r.Id == postId, Builders<Recipe>.Update
                        .Set(r => r.AverageRating, CalculateRatingAverage(recipe.RatingsCount, recipe.AverageRating, comment.Rating, commentDTO.Rating)));
                }
                else
                {
                    await _mongoService.CulinaryTips
                        .UpdateOneAsync(t => t.Id == postId, Builders<CulinaryTip>.Update
                        .Set(t => t.AverageRating, CalculateRatingAverage(tip.RatingsCount, tip.AverageRating, comment.Rating, commentDTO.Rating)));
                }
            }

            // Edit comment
            await _mongoService.Comments
                .UpdateOneAsync(c => c.Id == commentId, Builders<Comments>.Update
                .Set(c => c.Comment, commentDTO.Comment)
                .Set(c => c.Image, imageId)
                .Set(c => c.Rating, commentDTO.Rating)
                .Set(c => c.UpdatedAt, DateTime.Now));
        }

        private double? CalculateRatingAverage(int ratingCount, double ratingAverage, int? previousRating, int newRating)
        {
            if (previousRating == null)
            {
                return (ratingAverage * ratingCount + newRating) / (ratingCount + 1);
            }
            return (ratingAverage * ratingCount - previousRating + newRating) / ratingCount;
        }

        public async Task ReportPost(string postId, string? userId, ReportDTO reportDTO)
        {
            if(userId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Find recipe or tip that has the postId
            var recipe = (await _mongoService.Recipes
                .FindAsync(r => r.Id == postId))
                .FirstOrDefault();

            var tip = (await _mongoService.CulinaryTips
                .FindAsync(t => t.Id == postId))
                .FirstOrDefault();

            if (recipe == null && tip == null){
                throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Update status of post to Reported
            if (recipe != null)
            {
                await _mongoService.Recipes
                    .UpdateOneAsync(r => r.Id == postId, Builders<Recipe>.Update
                    .Set(r => r.Status, Status.Reported));
            }
            else
            {
                await _mongoService.CulinaryTips
                    .UpdateOneAsync(t => t.Id == postId, Builders<CulinaryTip>.Update
                    .Set(t => t.Status, Status.Reported));
            }

            // Add report to database
            await _mongoService.Reports
                .InsertOneAsync(new Report
                {
                    UserId = userId,
                    PostId = postId,
                    Reason = reportDTO.Reason
                });
        }

        public async Task ReportComment(string commentId, string? userId, ReportDTO reportDTO)
        {
            if(userId == null){
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Check if comment exists
            var comment = (await _mongoService.Comments
                .FindAsync(c => c.Id == commentId))
                .FirstOrDefault();

            if (comment == null){
                throw new NovaanException(ErrorCodes.COMMENT_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Update status of comment to Reported
            await _mongoService.Comments
                .UpdateOneAsync(c => c.Id == commentId, Builders<Comments>.Update
                .Set(c => c.Status, Status.Reported));

            // Add report to database
            await _mongoService.Reports
                .InsertOneAsync(new Report
                {
                    UserId = userId,
                    CommentId = commentId,
                    Reason = reportDTO.Reason
                });
        }
    }
}

