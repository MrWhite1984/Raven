using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.Models
{
    public class Category
    {
        public uint Id { get; set; }
        public string Title { get; set; }
        public Guid ImageFile {  get; set; }
        public uint PostCount { get; set; }

        public Category(uint  id, string title, Guid imageFile, uint postCount)
        {
            Id = id;
            Title = title;
            ImageFile = imageFile;
            PostCount = postCount;
        }
    }
}
