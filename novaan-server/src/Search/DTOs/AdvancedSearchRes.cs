
using MongoConnector.Enums;

namespace NovaanServer.src.Search.DTOs
{
    public class AdvancedSearchRes
    {
        public string Id { get; set; } = string.Empty;

        public string AuthorId { get; set; } = string.Empty;

        public string AuthorName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Thumbnails { get; set; } = string.Empty;

        public bool Saved { get; set; }

        public TimeSpan CookTime { get; set; }
    }
}