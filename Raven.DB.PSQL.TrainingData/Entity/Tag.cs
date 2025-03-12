namespace Raven.DB.PSQL.TrainingData.Entity
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<PostTags> PostTags { get; set; }
    }
}
