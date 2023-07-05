using MongoConnector.Enums;

namespace NovaanServer.src.Content.DTOs
{
    public class LikeReqDTO{
        public SubmissionType SubmissionType { get; set; }
        public bool Liked { get; set; }
    }
}