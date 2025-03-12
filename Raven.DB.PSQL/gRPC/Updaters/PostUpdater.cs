using Raven.DB.PSQL.Entity;

namespace Raven.DB.PSQL.gRPC.Updaters
{
    public class PostUpdater
    {
        public static async Task<(string, Posts)> UpdatePost(Posts posts)
        {
            (string, Posts) response = new();
            try
            {
                using (var db = new AppDbContext())
                {
                    response.Item2 = db.Posts.Update(posts).Entity;
                    await db.SaveChangesAsync();
                    response.Item1 = $"OK";
                }
                return response;
            }
            catch (Exception ex)
            {
                return (ex.Message, null);
            }
        }
    }
}
