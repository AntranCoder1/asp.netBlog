namespace Blog.ClassValue
{
    public class CommentValue
    {
        public string comment { get; set; }
        public string postId { get; set; }
        public string? parentCommentId { get; set; }
    }
}
