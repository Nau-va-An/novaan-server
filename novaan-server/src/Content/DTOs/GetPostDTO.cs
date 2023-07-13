using MongoConnector.Enums;

namespace NovaanServer.src.Content.DTOs
{
    public class GetPostDTO{
        public string PostId { get; set; }
        public SubmissionType PostType { get; set; }
    }
}