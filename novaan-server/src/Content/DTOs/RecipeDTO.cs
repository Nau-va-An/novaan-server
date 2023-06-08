using MongoConnector.Models;

namespace NovaanServer.src.Content.DTOs
{
    public class RecipeDTO
    {
        
        public string Title { get; set; }
        public IFormFile Video { get; set; }
        public List<IFormFile> Image { get; set; }
        public string Instructions { get; set; }
        public Difficulty Difficulty { get; set; } = Difficulty.Easy;
        public string PortionType { get; set; }
        
        public TimeSpan PrepTime { get; set; }
        public TimeSpan CookTime { get; set; }
        public string CreatorId { get; set; }
        public Status Status { get; set; }
    }
}
