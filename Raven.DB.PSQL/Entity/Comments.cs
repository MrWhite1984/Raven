namespace Raven.DB.PSQL.Entity
{
    public class Comments
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public string AuthorId { get; set; }
        public virtual Users User { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsRemoved { get; set; }
        public int LikesCount {  get; set; }
        public int CommentCount { get; set; }
        public virtual List<CommentContent> CommentContents { get; set; } = [];
    }
}
