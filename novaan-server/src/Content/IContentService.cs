using System;
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
        Task<GetReelDTO> GetPersonalReel(string? userId);
        Task LikePost(string postId, string? userId);
        Task SavePost(string postId, string? userId);
        Task CommentOnPost(string postId, string? userId, CommentDTO commentDTO);
        Task EditComment(string postId, string commentId, string? userId, CommentDTO commentDTO);
        Task ReportPost(string postId, string? userId, ReportDTO reportDTO);
        Task ReportComment(string commentId, string? userId, ReportDTO reportDTO);
    }
}

