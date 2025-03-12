using Raven.DB.PSQL.Entity;

namespace Raven.DB.PSQL.gRPC.Importers
{
    public class CommentContentImporter
    {
        public static async Task<(string, CommentContent)> CreateCommentContent(CommentContent commentContent)
        {
            (string, CommentContent) response = new();
            try
            {
                using (var db = new AppDbContext())
                {
                    response.Item2 = db.CommentContent.Add(commentContent).Entity;
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
