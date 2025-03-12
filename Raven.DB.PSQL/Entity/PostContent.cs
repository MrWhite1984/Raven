using Raven.DB.PSQL.Entity.@enum;

namespace Raven.DB.PSQL.Entity
{
    public class PostContent
    {
        public Guid PostId { get; set; }
        public virtual Posts Post { get; set; }
        public Guid ContentId { get; set; }
        public string Marker { get; set; }
        public ContentType ContentType { get; set; }
    }
}
