using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.Models
{
    public class TagsPosts
    {
        public Guid PostId { get; set; }
        public uint TagId { get; set; }
    }
}
