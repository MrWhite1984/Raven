using Microsoft.EntityFrameworkCore;
using Raven.DB.PSQL.Entity;

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
                    var postContents = db.PostContents
                        .Where(o => o.PostId == postId)
                        .ToListAsync();

                    return (postContents.Result, "OK");
                }
            }
            catch (Exception ex)
            {
                return (new List<PostContent>(), ex.Message);
            }
        }
    }
}
