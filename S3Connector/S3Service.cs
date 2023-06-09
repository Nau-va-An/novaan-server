using System;
using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Amazon.S3.Model;
using System.Net;
using Microsoft.AspNetCore.WebUtilities;

namespace S3Connector
{
    public class S3Service
    {
        private readonly string _bucketName;
        private readonly string _region;

        private readonly AmazonS3Client _s3Client;

        public S3Service()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = builder.Build();

            // TODO: Refactor this?
            var bucketName = config.GetSection("S3:BucketName").Value;
            var region = config.GetSection("S3:Region").Value;

            var accessKey = config.GetSection("S3:AccessKey").Value;
            var secretKey = config.GetSection("S3:SecretKey").Value;

            if (bucketName == null || region == null ||
                accessKey == null || secretKey == null)
            {
                throw new Exception("Cannot read S3 settings in appsettings");
            }

            _bucketName = bucketName;
            _region = region;
            _s3Client = new(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(_region));

        }

        // TODO: Demo S3 upload file, fix if needed
        // TODO: Add error handling
        public async Task<bool> UploadFileAsync(IFormFile file)
        {
            var request = new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = System.Guid.NewGuid().ToString() + Path.GetExtension(file.FileName),
                InputStream = file.OpenReadStream()
            };
            request.Metadata.Add("Content-Type", file.ContentType);

            var response = await _s3Client.PutObjectAsync(request);
            return true;
        }

        public Task UploadFileAsync(byte[] fileStream, MultipartSection section)
        {
            var request = new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = System.Guid.NewGuid().ToString() + Path.GetExtension(section.AsFileSection().FileName),
                InputStream = new MemoryStream(fileStream)
            };
            request.Metadata.Add("Content-Type", section.ContentType);
            return _s3Client.PutObjectAsync(request);
        }
    }
}

