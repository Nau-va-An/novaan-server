using System;
using MongoConnector.Models;
using NovaanServer.src.Content.DTOs;

namespace NovaanServer.src.Content
{
    public interface IContentService
    {
        Task AddCulinaryTips(CulinaryTips culinaryTips);
        Task UploadRecipe(Recipes recipe);
        Task<T> ProcessMultipartRequest<T>(HttpRequest request);
        Task ValidateFileMetadata(FileInformationDTO fileMetadata);
    }
}

