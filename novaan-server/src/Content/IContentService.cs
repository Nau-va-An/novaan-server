using System;
using MongoConnector.Models;
using NovaanServer.src.Content.DTOs;

namespace NovaanServer.src.Content
{
    public interface IContentService
    {
        Task AddCulinaryTips(CulinaryTip culinaryTips);
        Task UploadRecipe(Recipe recipe);
        Task<T> ProcessMultipartRequest<T>(HttpRequest request);
        bool ValidateFileMetadata(FileInformationDTO fileMetadata);
        PostDTO GetPosts();
    }
}

