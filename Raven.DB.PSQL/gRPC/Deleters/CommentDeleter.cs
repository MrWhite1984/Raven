namespace Raven.DB.PSQL.gRPC.Deleters
{
    public class CommentDeleter
    {
        public static async Task<string> DeleteComment(Guid id)
        {
            string response = "";
            try
            {
                using (var db = new AppDbContext())
                {
                    db.Comments.Remove(await db.Comments.FindAsync(id));
                    await db.SaveChangesAsync();
                    response = $"OK";
                    return response;
                }
            }
            catch (Exception ex)
            {
                return (ex.Message);
            }
        }
    }
}
