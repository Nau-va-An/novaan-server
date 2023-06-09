using System;
using MongoConnector;
using MongoConnector.Models;
using NovaanServer.src.ExceptionLayer.CustomExceptions;

namespace NovaanServer.src.Content
{
    public class ContentService : IContentService
    {
        private readonly MongoDBService _mongoService;
        public ContentService(MongoDBService mongoDBService)
        {
            _mongoService = mongoDBService;
        }
        public async Task AddCulinaryTips(CulinaryTips culinaryTips)
        {
            try
            {
                await _mongoService.CulinaryTips.InsertOneAsync(culinaryTips);
            }
            catch (System.Exception)
            {
                throw new Exception(ExceptionMessage.SERVER_UNAVAILABLE);
            }
        }
    }
}

