using System.ComponentModel.DataAnnotations;
using System.Drawing;
namespace NovaanServer.src.Image.DTOs
{
    public class UploadImageDTO
    {
        [Required]
        public List<IFormFile> Images { get; set; }
    }
}
