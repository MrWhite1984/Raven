using Raven.Entity.@enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.Entity
{
    public class CommentContent
    {
        public Guid CommentId { get; set; }
        public virtual Comments Comment { get; set; }
        public Guid ContentId { get; set; }
        public string Marker { get; set; }
        public ContentType ContentType { get; set; }
    }
}
