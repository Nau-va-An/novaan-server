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
        Task<PostDTO> GetPosts();
    }
}

