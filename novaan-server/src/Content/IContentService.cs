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
        PostDTO GetPosts();
        Task LikePost(string postId, string? userId);
        Task SavePost(string postId, string? userId);
        Task CommentOnPost(string postId, string? userId, CommentDTO commentDTO);
        Task EditComment(string postId, string? userId, CommentDTO commentDTO);
    }
}

