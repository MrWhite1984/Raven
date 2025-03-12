namespace Raven.DB.PSQL.Entity
{
    public class Categories
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Guid ImageFile {  get; set; }
        public int PostCount { get; set; }
        public virtual List<Posts> Posts { get; set; }
    }
}
