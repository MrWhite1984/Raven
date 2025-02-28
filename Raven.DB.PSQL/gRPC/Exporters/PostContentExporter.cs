using Microsoft.EntityFrameworkCore;
using Raven.DB.PSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.gRPC.Exporters
{
    public class PostContentExporter
    {
        public static async Task<(List<PostContent>, string)> GetContentPost(Guid postId)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var tags = db.PostContents
                        .Where(o => o.PostId == postId)
                        .ToListAsync();

                    return (tags.Result, "OK");
                }
            }
            catch (Exception ex)
            {
                return (new List<PostContent>(), ex.Message);
            }
        }
    }
}
