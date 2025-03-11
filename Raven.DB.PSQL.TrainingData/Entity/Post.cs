using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.TrainingData.Entity
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public virtual List<PostTags> PostTags { get; set; } = [];
    }
}
