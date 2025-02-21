using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.Entity
{
    public class Categories
    {
        public uint Id { get; set; }
        public string Title { get; set; }
        public Guid ImageFile {  get; set; }
        public uint PostCount { get; set; }
        public virtual List<Posts> Posts { get; set; }
    }
}
