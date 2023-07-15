using System;
using MongoConnector.Enums;
using MongoConnector.Models;
using NovaanServer.src.Content.DTOs;

namespace NovaanServer.src.Content
{
    public interface IContentService
    {
        Task UploadTips(CulinaryTip culinaryTips, string userId);
        Task UploadRecipe(Recipe recipe, string userId);

        Task<T> ProcessMultipartRequest<T>(HttpRequest request);
        bool ValidateFileMetadata(FileInformationDTO fileMetadata);
        Task LikePost(string postId, string userId, LikeReqDTO likeDTO);
        Task SavePost(string postId, string userId, SubmissionType postType);
      
        Task CommentOnPost(string postId, string userId, CommentDTO commentDTO);
        Task EditComment(string postId, string userId, CommentDTO commentDTO);
        Task DeleteComment(string postId, SubmissionType postType, string userId);

        Task ReportPost(string postId, string userId, ReportDTO reportDTO);
        Task ReportComment(string commentId, string userId, ReportDTO reportDTO);
      
        Task<List<GetPostDTO>> GetPersonalReel(string userId);
        Task<GetTipsDetailDTO> GetCulinaryTip(string postId, string currentUserId);
        Task<GetRecipeDetailDTO> GetRecipe(string postId, string currentUserId);
        Task<List<GetPostCommentsDTO>> GetComments(string postId, string currentUserId);
    }
}

