using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.Entity
{
    public class Users
    {
        public string Id { get; set; }
        public virtual List<Posts> Posts { get; set; }
        public virtual List<Comments> Comments { get; set; }
    }
}
