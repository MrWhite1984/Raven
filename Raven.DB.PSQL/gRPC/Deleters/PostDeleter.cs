namespace Raven.DB.PSQL.gRPC.Deleters
{
    public class PostDeleter
    {
        public static async Task<string> DeletePost(Guid id)
        {
            string response = "";
            try
            {
                using (var db = new AppDbContext())
                {
                    db.Posts.Remove(await db.Posts.FindAsync(id));
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
