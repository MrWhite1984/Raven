using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.Entity
{
    public class Posts
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public int CategoryId { get; set; }
        public virtual Categories CategoryPost { get; set; }
        public int LikesCount { get; set; }
        public int CommentCount { get; set; }
        public int ViewsCount { get; set; }
        public int BookmarksCount { get; set; }
        public string AuthorId { get; set; }
        public virtual Users User { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public float[] Vectors { get; set; }
        public virtual List<TagsPosts> TagsPosts { get; set; } = [];
        public virtual List<PostContent> PostContents { get; set; } = [];

    }
}
