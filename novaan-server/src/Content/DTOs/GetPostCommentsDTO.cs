namespace NovaanServer.src.Content.DTOs
{
    public class GetPostCommentsDTO
    {
        public string CommentID { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }
        
        public string Avatar { get; set; }

        public string Comment { get; set; }

        public string Image { get; set; }

        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}