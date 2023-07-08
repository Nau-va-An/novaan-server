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

        public async Task<List<GetPostDTO>> GetPersonalReel(string? userId)
        {
            try
            {
                // TODO: Get all id of both recipe and tips 
                // add them to postIds order by updatedAt
                // return all posts with id and post type

                var recipes = await _mongoService.Recipes
                    .Find(r => r.Status == Status.Approved)
                    .Project(r => new { r.Id, r.UpdatedAt, PostType = SubmissionType.Recipe })
                    .ToListAsync();

                var tips = await _mongoService.CulinaryTips
                    .Find(t => t.Status == Status.Approved)
                    .Project(t => new { t.Id, t.UpdatedAt, PostType = SubmissionType.CulinaryTip })
                    .ToListAsync();

                var allPosts = recipes.Concat(tips).ToList();
                allPosts.Sort((x, y) => y.UpdatedAt.CompareTo(x.UpdatedAt));

                return allPosts.Select(post => new GetPostDTO
                {
                    PostId = post.Id,
                    PostType = post.PostType
                }).ToList();

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

            var extension = Path.GetExtension(section.FileName)
                ?? throw new NovaanException(ErrorCodes.CONTENT_EXT_INVALID, HttpStatusCode.BadRequest);
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

        public async Task LikePost(string postId, string? userId, LikeReqDTO likeDTO)
        {
            if (userId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            var updates = Builders<Likes>.Update
                .Set(l => l.UserId, userId)
                .Set(l => l.PostId, postId)
                .BitwiseXor(l => l.Liked, 1)
                .Set(l => l.PostType, likeDTO.PostType);

            // If there is a document contain userId and postId, it will be updated
            var response = await _mongoService.Likes
                .UpdateOneAsync(
                    l => l.PostId == postId &&
                    l.UserId == userId &&
                    l.PostType == SubmissionType.Recipe,
                updates,
                // If not, it will be inserted
                new UpdateOptions { IsUpsert = true });

            // Update likeCount according to upsert result
            bool isLiked = response.UpsertedId != null;
            switch (likeDTO.PostType)
            {
                case SubmissionType.Recipe:
                    await _mongoService.Recipes
                        .UpdateOneAsync(
                            r => r.Id == postId,
                            Builders<Recipe>.Update.Inc(r => r.LikesCount, isLiked ? 1 : -1)
                        );
                    break;

                case SubmissionType.CulinaryTip:
                    await _mongoService.CulinaryTips
                        .UpdateOneAsync(
                            t => t.Id == postId,
                            Builders<CulinaryTip>.Update.Inc(t => t.LikesCount, isLiked ? 1 : -1)
                        );
                    break;

                default:
                    throw new NovaanException(ErrorCodes.SUBMISSION_TYPE_INVALID, HttpStatusCode.BadRequest);
            }
        }

        public async Task SavePost(string postId, string? userId, SubmissionType postType)
        {
            if (userId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Switchcase for handle submissionType
            switch (postType)
            {
                case SubmissionType.Recipe:
                    var recipe = (await _mongoService.Recipes
                        .FindAsync(r => r.Id == postId && r.Status == Status.Approved))
                        .FirstOrDefault()
                        ?? throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);
                    break;

                case SubmissionType.CulinaryTip:
                    var tip = (await _mongoService.CulinaryTips
                        .FindAsync(t => t.Id == postId && t.Status == Status.Approved))
                        .FirstOrDefault()
                        ?? throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);
                    break;

                default:
                    throw new NovaanException(ErrorCodes.SUBMISSION_TYPE_INVALID, HttpStatusCode.BadRequest);
            }

            // Check if user has saved this post before in savedPost list of user collection
            bool hasSavedPost = (await _mongoService.Users
                .FindAsync(
                    u => u.Id == userId &&
                    u.SavedPosts.Any(p => p.PostId == postId && p.PostType == postType))
                )
                .FirstOrDefault() != null;

            if (!hasSavedPost)
            {
                // Add the post to user saved posts list
                var updates = Builders<User>.Update.Push(
                    u => u.SavedPosts,
                    new SavedPost
                    {
                        PostId = postId,
                        PostType = postType
                    }
                );
                await _mongoService.Users
                    .UpdateOneAsync(u => u.Id == userId, updates);
            }
            else
            {
                // Remove the post from user saved posts list
                var updates = Builders<User>.Update.PullFilter(
                    u => u.SavedPosts,
                    sp => sp.PostId == postId && sp.PostType == postType
                );
                await _mongoService.Users.UpdateOneAsync(u => u.Id == userId, updates);
            }
        }

        public async Task CommentOnPost(string postId, string? userId, CommentDTO commentDTO)
        {
            if (userId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Check if user has commented on this post before
            var hasCommented = (await _mongoService.Comments
                .FindAsync(c => c.PostId == postId && c.UserId == userId && c.postType == commentDTO.PostType))
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
                    postType = commentDTO.PostType,
                    Comment = commentDTO.Comment,
                    CreatedAt = DateTime.Now,
                    Image = imageId,
                    Rating = commentDTO.Rating
                });

            switch (commentDTO.PostType)
            {
                case SubmissionType.Recipe:
                    var recipe = (await _mongoService.Recipes
                        .FindAsync(r => r.Id == postId && r.Status == Status.Approved))
                        .FirstOrDefault()
                        ?? throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);

                    // Update ratingCount and rating average
                    await _mongoService.Recipes
                        .UpdateOneAsync(r => r.Id == postId, Builders<Recipe>.Update
                        .Inc(r => r.RatingsCount, 1)
                        .Set(r => r.AverageRating,
                            CalculateRatingAverage(
                                    recipe.RatingsCount,
                                    recipe.AverageRating,
                                    null,
                                    commentDTO.Rating)
                        ));
                    break;

                case SubmissionType.CulinaryTip:
                    var tip = (await _mongoService.CulinaryTips
                        .FindAsync(t => t.Id == postId && t.Status == Status.Approved))
                        .FirstOrDefault()
                        ?? throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);

                    // Update ratingCount and rating average
                    await _mongoService.CulinaryTips
                        .UpdateOneAsync(t => t.Id == postId, Builders<CulinaryTip>.Update
                        .Inc(t => t.RatingsCount, 1)
                        .Set(t => t.AverageRating,
                            CalculateRatingAverage(
                                    tip.RatingsCount,
                                    tip.AverageRating,
                                    null,
                                    commentDTO.Rating)
                        ));
                    break;

                default:
                    throw new NovaanException(ErrorCodes.SUBMISSION_TYPE_INVALID, HttpStatusCode.BadRequest);
            }
        }

        public async Task EditComment(string postId, string? userId, CommentDTO commentDTO)
        {
            if (userId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // get comment of user on this post
            var comment = (await _mongoService.Comments
                .FindAsync(c => c.PostId == postId && c.UserId == userId && c.postType == commentDTO.PostType))
                .FirstOrDefault()
                ?? throw new NovaanException(ErrorCodes.COMMENT_NOT_FOUND, HttpStatusCode.BadRequest);

            string imageId = comment.Image ?? "";

            if (!string.IsNullOrEmpty(commentDTO.ExistingImage) && commentDTO.ExistingImage != comment.Image)
            {
                throw new NovaanException(ErrorCodes.CONTENT_IMAGE_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Case 1: User remove image from comment 
            if (commentDTO.Image == null && !string.IsNullOrEmpty(comment.Image) && commentDTO.ExistingImage == null)
            {
                // Delete image from S3
                await _s3Service.DeleteFileAsync(comment.Image);
                imageId = null;
            }

            // Case 2: User replace image by new image
            if (commentDTO.Image != null)
            {
                // Delete old image from S3
                await _s3Service.DeleteFileAsync(comment.Image);

                // Upload new image to S3
                imageId = System.Guid.NewGuid().ToString() +
                               Path.GetExtension(commentDTO.Image.FileName);
                await _s3Service.UploadFileAsync(commentDTO.Image.OpenReadStream(), imageId);
            }

            // Update comment
            await _mongoService.Comments
                .UpdateOneAsync(c => c.Id == comment.Id, Builders<Comments>.Update
                .Set(c => c.Comment, commentDTO.Comment)
                .Set(c => c.Image, imageId)
                .Set(c => c.Rating, commentDTO.Rating)
                .Set(c => c.UpdatedAt, DateTime.Now));

            switch (commentDTO.PostType)
            {
                case SubmissionType.Recipe:
                    var recipe = (await _mongoService.Recipes
                        .FindAsync(r => r.Id == postId && r.Status == Status.Approved))
                        .FirstOrDefault()
                        ?? throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);

                    // Update rating average of post with new rating from user and ratingaverage of post before edit
                    if (comment.Rating != commentDTO.Rating)
                    {
                        await _mongoService.Recipes
                       .UpdateOneAsync(r => r.Id == postId, Builders<Recipe>.Update
                       .Set(r => r.AverageRating,
                           CalculateRatingAverage(
                               recipe.RatingsCount,
                               recipe.AverageRating,
                               comment.Rating,
                               commentDTO.Rating)
                       ));
                    }
                    break;

                case SubmissionType.CulinaryTip:
                    var tip = (await _mongoService.CulinaryTips
                        .FindAsync(t => t.Id == postId && t.Status == Status.Approved))
                        .FirstOrDefault()
                        ?? throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);

                    // Update rating average of post with new rating from user and ratingaverage of post before edit
                    if (comment.Rating != commentDTO.Rating)
                    {
                        await _mongoService.CulinaryTips
                        .UpdateOneAsync(t => t.Id == postId, Builders<CulinaryTip>.Update
                        .Set(t => t.AverageRating,
                            CalculateRatingAverage(
                                tip.RatingsCount,
                                tip.AverageRating,
                                comment.Rating,
                                commentDTO.Rating)
                        ));
                    }
                    break;

                default:
                    throw new NovaanException(ErrorCodes.SUBMISSION_TYPE_INVALID, HttpStatusCode.BadRequest);
            }
        }

        public async Task DeleteComment(string postId, SubmissionType submissionType, string? userId)
        {
            if (userId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // get comment of user on this post
            var comment = (await _mongoService.Comments
                .FindAsync(c => c.PostId == postId && c.UserId == userId && c.postType == submissionType))
                .FirstOrDefault()
                ?? throw new NovaanException(ErrorCodes.COMMENT_NOT_FOUND, HttpStatusCode.BadRequest);

            switch (submissionType)
            {
                case SubmissionType.Recipe:
                    var recipe = (await _mongoService.Recipes
                        .FindAsync(r => r.Id == postId && r.Status == Status.Approved))
                        .FirstOrDefault()
                        ?? throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);

                    // Update rating average of post because the rating of this comment will be removed
                    await _mongoService.Recipes
                        .UpdateOneAsync(r => r.Id == postId, Builders<Recipe>.Update
                        .Set(r => r.AverageRating,
                            CalculateRatingAverage(
                                recipe.RatingsCount,
                                recipe.AverageRating,
                                comment.Rating,
                                null)
                        )
                        .Set(r => r.RatingsCount, recipe.RatingsCount - 1));
                    break;

                case SubmissionType.CulinaryTip:
                    var tip = (await _mongoService.CulinaryTips
                        .FindAsync(t => t.Id == postId && t.Status == Status.Approved))
                        .FirstOrDefault()
                        ?? throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);

                    // Update rating average of post because the rating of this comment will be removed
                    await _mongoService.CulinaryTips
                        .UpdateOneAsync(t => t.Id == postId, Builders<CulinaryTip>.Update
                        .Set(t => t.AverageRating,
                            CalculateRatingAverage(
                                tip.RatingsCount,
                                tip.AverageRating,
                                comment.Rating,
                                null)
                        )
                        .Set(t => t.RatingsCount, tip.RatingsCount - 1));
                    break;

                default:
                    throw new NovaanException(ErrorCodes.SUBMISSION_TYPE_INVALID, HttpStatusCode.BadRequest);
            }

            // Delete comment
            await _mongoService.Comments
                .DeleteOneAsync(c => c.Id == comment.Id);
        }

        private double? CalculateRatingAverage(int ratingCount, double ratingAverage, int? previousRating, int? newRating)
        {
            // Case 1: User has not rated this post before and add new rating
            if (previousRating == null)
            {
                return (ratingAverage * ratingCount + newRating) / (ratingCount + 1);
            }

            // Case 2: User has rated this post before and remove rating
            if (newRating == null)
            {
                return (ratingAverage * ratingCount - previousRating) / (ratingCount - 1);
            }
            // Case 3: User has rated this post before and change rating
            return (ratingAverage * ratingCount - previousRating + newRating) / ratingCount;
        }

        public async Task ReportPost(string postId, string? userId, ReportDTO reportDTO)
        {
            if (userId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            switch (reportDTO.PostType)
            {
                case SubmissionType.Recipe:
                    var recipe = (await _mongoService.Recipes
                        .FindAsync(r => r.Id == postId && r.Status == Status.Approved))
                        .FirstOrDefault()
                        ?? throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);

                    break;

                case SubmissionType.CulinaryTip:
                    var tip = (await _mongoService.CulinaryTips
                        .FindAsync(t => t.Id == postId && t.Status == Status.Approved))
                        .FirstOrDefault()
                        ?? throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.BadRequest);

                    break;

                default:
                    throw new NovaanException(ErrorCodes.SUBMISSION_TYPE_INVALID, HttpStatusCode.BadRequest);
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
            if (userId == null)
            {
                throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.BadRequest);
            }

            // Check if comment exists
            var comment = (await _mongoService.Comments
                .FindAsync(c => c.Id == commentId))
                .FirstOrDefault();

            if (comment == null)
            {
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

        public async Task<GetTipsDetailDTO> GetCulinaryTip(string postId, string currentUserId)
        {
            // Get tips
            var tip = (await _mongoService.CulinaryTips
                .FindAsync(t => t.Id == postId))
                .FirstOrDefault();

            if (tip == null || tip.CreatorId != currentUserId && tip.Status != Status.Approved)
            {
                throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.NotFound);
            }

            // Check if user has liked this post
            bool isLiked = await IsLiked(postId, currentUserId);

            // Check if user has saved this post
            bool isSaved = await IsSaved(postId, currentUserId);

            AdminComment? latestComment = null;
            if (tip.AdminComments.Count > 0 &&
                (tip.Status == Status.Rejected || tip.Status == Status.Reported)
            )
            {
                latestComment = tip.AdminComments.Last();
            }
            GetTipsDetailDTO getTipsDetailDTO = new()
            {
                Id = tip.Id,
                CreatorId = tip.CreatorId,
                Title = tip.Title,
                Description = tip.Description,
                Video = tip.Video,
                Status = tip.Status,
                CreatedAt = tip.CreatedAt,
                AdminComment = latestComment,
                IsLiked = isLiked,
                IsSaved = isSaved
            };
            return getTipsDetailDTO;
        }

        public async Task<GetRecipeDetailDTO> GetRecipe(string postId, string currentUserId)
        {
            // Get recipe
            var recipe = (await _mongoService.Recipes
                .FindAsync(r => r.Id == postId))
                .FirstOrDefault()
                ?? throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.NotFound);

            if (currentUserId != recipe.CreatorId && recipe.Status != Status.Approved)
            {
                throw new NovaanException(ErrorCodes.CONTENT_NOT_FOUND, HttpStatusCode.NotFound);
            }

            // Check if user has liked this post
            bool isLiked = await IsLiked(postId, currentUserId);

            // Check if user has saved this post
            bool isSaved = await IsSaved(postId, currentUserId);

            AdminComment? latestComment = null;
            if (recipe.AdminComments.Count > 0 &&
                (recipe.Status == Status.Rejected || recipe.Status == Status.Reported)
            )
            {
                latestComment = recipe.AdminComments.Last();
            }

            // Get creator name
            var creatorName = (await _mongoService.Users
                .FindAsync(u => u.Id == recipe.CreatorId))
                .FirstOrDefault()?.DisplayName ?? string.Empty;

            GetRecipeDetailDTO getRecipeDetailDTO = new()
            {
                Id = recipe.Id,
                CreatorId = recipe.CreatorId,
                CreatorName = creatorName,
                Title = recipe.Title,
                Description = recipe.Description,
                Difficulty = (int)recipe.Difficulty,
                PortionType = (int)recipe.PortionType,
                Video = recipe.Video,
                Status = recipe.Status,
                CreatedAt = recipe.CreatedAt,
                CookTime = recipe.CookTime,
                PrepTime = recipe.PrepTime,
                Ingredients = recipe.Ingredients,
                Instructions = recipe.Instructions,
                AdminComment = latestComment,
                IsLiked = isLiked,
                IsSaved = isSaved
            };

            return getRecipeDetailDTO;
        }

        private async Task<bool> IsSaved(string postId, string? currentUserId)
        {
            // Check if user had saved this post
            return (await _mongoService.Users
                .FindAsync(u => u.Id == currentUserId && u.SavedPosts.Any(p => p.PostId == postId)))
                .FirstOrDefault() != null;
        }

        private async Task<bool> IsLiked(string id, string? currentUserId)
        {
            // Check if user had liked this post
            return (await _mongoService.Likes
                .FindAsync(l => l.UserId == currentUserId && l.PostId == id))
                .FirstOrDefault() != null;
        }
    }
}

