using System;
using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Amazon.S3.Model;
using System.Net;
using Microsoft.AspNetCore.WebUtilities;
using Amazon.S3.Transfer;

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
        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var request = new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = System.Guid.NewGuid().ToString() + Path.GetExtension(file.FileName),
                InputStream = file.OpenReadStream()
            };
            request.Metadata.Add("Content-Type", file.ContentType);

            await _s3Client.PutObjectAsync(request);
            return request.Key;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                var fileTransferUtils = new TransferUtility(_s3Client);
                var fileTransferReq = new TransferUtilityUploadRequest
                {
                    BucketName = _bucketName,
                    Key = fileName,
                    InputStream = fileStream,
                };

                await fileTransferUtils.UploadAsync(fileTransferReq);
                return fileTransferReq.Key;
            }
            catch
            {
                throw;
            }
            finally
            {
                // Always close the stream for safety
                fileStream.Close();
            }

        }

        // Return a limited time download link
        public string DownloadFileAsync(string keyId)
        {
            var downloadReq = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = keyId,
                // Add this time to appsettings to avoid hard-coded value
                Expires = DateTime.UtcNow.AddMinutes(5)
            };

            try
            {
                return _s3Client.GetPreSignedURL(downloadReq);
            }
            catch
            {
                // Throw custom error here
                return "";
            }

        }
    }
}

