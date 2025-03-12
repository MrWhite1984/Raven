namespace Raven.DB.PSQL.TrainingData.Entity
{
    public class PostTags
    {
        public virtual Post Post { get; set; }
        public Guid PostId { get; set; }
        public virtual Tag Tag { get; set; }
        public int TagId { get; set; }
    }
}
