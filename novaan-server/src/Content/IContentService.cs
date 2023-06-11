using System;
using MongoConnector.Models;
using NovaanServer.src.Content.DTOs;

namespace NovaanServer.src.Content
{
    public interface IContentService
    {
        Task AddCulinaryTips(CulinaryTips culinaryTips);
        Task<Dictionary<string,object>> ProcessMultipartRequest(HttpRequest request);
        Task ValidateFileMetadata(FileInformationDTO fileMetadata);
    }
}

