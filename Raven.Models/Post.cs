using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.Models
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public uint CategoryId { get; set; }
        public uint LikesCount { get; set; }
        public uint CommentCount { get; set; }
        public uint ViewsCount { get; set; }
        public uint BookmarksCount { get; set; }
        public string AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public float[] Vectors { get; set; }

    }
}
