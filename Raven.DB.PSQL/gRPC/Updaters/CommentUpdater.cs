using Raven.DB.PSQL.Entity;

namespace Raven.DB.PSQL.gRPC.Updaters
{
    public class CommentUpdater
    {
        public static async Task<(string, Comments)> UpdateComment(Comments comment)
        {
            (string, Comments) response = new();
            try
            {
                using (var db = new AppDbContext())
                {
                    response.Item2 = db.Comments.Update(comment).Entity;
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
