using NovaanServer.src.Image.DTOs;

namespace NovaanServer.src.Image
{
    public interface IImageService
    {
        public Task<IEnumerable<string>> UploadImage(UploadImageDTO images);

    }

}
