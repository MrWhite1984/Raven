using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.Entity
{
    public class Tags
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public virtual List<TagsPosts> TagsPosts { get; set; }
    }
}
