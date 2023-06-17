using System;
using MongoConnector;
using S3Connector;

namespace NovaanServer.Developer
{
    public class DevService : IDevService
    {
        private readonly ILogger _logger;

        private readonly MongoDBService _mongoService;
        private readonly S3Service _s3Service;

        public DevService(ILogger<DevService> logger,
            MongoDBService mongoDBService,
            S3Service s3Service)
        {
            _logger = logger;

            _mongoService = mongoDBService;
            _s3Service = s3Service;
        }

        public bool IsDatabaseHealthy()
        {
            if (_mongoService == null)
            {
                _logger.LogError("Database service is missing");
                return false;
            }

            return _mongoService.PingDatabase();
        }

        public bool IsS3Healthy()
        {
            if (_s3Service == null)
            {
                _logger.LogError("S3 service is missing");
                return false;
            }

            return true;
        }

        public bool IsServerHealthy()
        {
            return true;
        }
    }
}

