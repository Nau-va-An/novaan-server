using System;
using MongoConnector.Models;

namespace NovaanServer.src.Content
{
    public interface IContentService
    {
        Task AddCulinaryTips(CulinaryTips culinaryTips);
        Task<CulinaryTips> ProcessMultipartRequest(HttpRequest request);
    }
}

