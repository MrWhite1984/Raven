using Raven.Entity.@enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.Entity
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
