using NovaanServer.src.Image.DTOs;
using S3Connector;
using System.Collections.Generic;

namespace NovaanServer.src.Image
{
    public class ImageService : IImageService
    {
        private readonly S3Service _s3Service;
        public ImageService(S3Service s3Service)
        {
            _s3Service = s3Service;
        }
        
        public async Task<IEnumerable<string>>  UploadImage(UploadImageDTO images)
        {
            List<string> imageIDs = new();
            string id;
            try
            { 
                foreach (var image in images.Images) { 
                    id = _s3Service.UploadFileAsync(image).Result;
                    imageIDs.Add(id);
                 }
            } catch (TaskCanceledException ex) {
                throw new TaskCanceledException(ex.Message);
            }
            return imageIDs;
        }
    }
}
