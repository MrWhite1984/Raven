using Raven.DB.PSQL.Entity;

namespace Raven.DB.PSQL.gRPC.Importers
{
    public class PostContentImporter
    {
        public static async Task<(string, PostContent)> CreatePostContent(PostContent postContent)
        {
            (string, PostContent) response = new();
            try
            {
                using (var db = new AppDbContext())
                {
                    response.Item2 = db.PostContents.Add(postContent).Entity;
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
