using Microsoft.AspNetCore.Mvc;
using NovaanServer.Auth;
using NovaanServer.src.Image.DTOs;

namespace NovaanServer.src.Image
{
    [Route("api/images")]
    [ApiController]
    public class ImageController : Controller
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImages([FromForm] UploadImageDTO imageDTO)
        {
           
                await _imageService.UploadImage(imageDTO);
                return Ok();
         
        }
    }
}
    

