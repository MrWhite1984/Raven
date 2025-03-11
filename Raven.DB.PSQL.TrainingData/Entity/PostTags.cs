using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.TrainingData.Entity
{
    public class PostTags
    {
        public virtual Post Post { get; set; }
        public Guid PostId { get; set; }
        public virtual Tag Tag { get; set; }
        public int TagId { get; set; }
    }
}
