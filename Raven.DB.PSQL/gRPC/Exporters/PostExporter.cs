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
                        .Include(o => o.TagsPosts)
                        .ThenInclude(o => o.Tag)
                        .Include(o => o.PostContents)
                        .Include(o => o.User)
                        .Include(o => o.CategoryPost)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    return (post, "OK");
                }
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        public static async Task<(List<Posts>, string, DateTime)> GetPosts(DateTime cursor, int pageSize)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var posts = await db.Posts
                        .Include(o => o.TagsPosts)
                        .ThenInclude(o => o.Tag)
                        .Include(o => o.PostContents)
                        .Include(o => o.User)
                        .Include(o => o.CategoryPost)
                        .OrderByDescending(p => p.Id)
                        .Where(o => o.CreatedAt <= cursor)
                        .Take(pageSize+1)
                        .ToListAsync();
                    return (posts.Take(pageSize).ToList(), "OK", posts.Last().CreatedAt);
                }
            }
            catch(Exception ex)
            {
                return (new List<Posts>(), ex.Message, DateTime.MinValue);
            }
        }
    }
}
