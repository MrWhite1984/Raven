namespace Raven.DB.PSQL.Entity
{
    public class TagsPosts
    {
        public virtual Posts Post { get; set; }
        public Guid PostId { get; set; }
        public virtual Tags Tag { get; set; }
        public int TagId { get; set; }
    }
}
