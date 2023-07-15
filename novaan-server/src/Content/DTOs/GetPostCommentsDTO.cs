namespace NovaanServer.src.Content.DTOs
{
    public class GetPostCommentsDTO
    {
        public string CommentId { get; set; }

        public string UserId { get; set; }

        public string Username { get; set; }
        
        public string Avatar { get; set; }

        public string Comment { get; set; }

        public string Image { get; set; }

        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}