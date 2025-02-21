using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public int CommentsCount { get; set; }
        public string AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsRemoved { get; set; }
        public uint LikesCount {  get; set; }
    }
}
