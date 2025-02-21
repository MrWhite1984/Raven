using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.Entity
{
    public class TagsPosts
    {
        public virtual Posts Post { get; set; }
        public Guid PostId { get; set; }
        public virtual Tags Tag { get; set; }
        public uint TagId { get; set; }
    }
}
