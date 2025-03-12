namespace Raven.DB.PSQL.Entity
{
    public class Tags
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<TagsPosts> TagsPosts { get; set; }
    }
}
