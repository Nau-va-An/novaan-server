using MongoConnector.Enums;

namespace NovaanServer.src.Search.DTOs
{
    public class GetRecipesByIngredientsRes
    {
        public string Id { get; set; }

        public string AuthorId { get; set; }

        public string AuthorName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Thumbnails { get; set; } = string.Empty;

        public bool Liked { get; set; }

        public TimeSpan CookTime { get; set; }
    }
}