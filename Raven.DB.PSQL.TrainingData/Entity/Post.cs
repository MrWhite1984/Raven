namespace Raven.DB.PSQL.TrainingData.Entity
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public virtual List<PostTags> PostTags { get; set; } = [];
    }
}
