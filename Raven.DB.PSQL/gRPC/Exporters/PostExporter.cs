using Microsoft.EntityFrameworkCore;
using Raven.DB.PSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.gRPC.Exporters
{
    public class PostExporter
    {
        public static async Task<(Posts, string)> GetPost(Guid id)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    Posts post = await db.Posts
                        .FirstOrDefaultAsync(p => p.Id == id);

                    return (post, "OK");
                }
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
    }
}
