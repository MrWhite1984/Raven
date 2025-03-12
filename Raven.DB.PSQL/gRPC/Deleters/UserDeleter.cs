namespace Raven.DB.PSQL.gRPC.Deleters
{
    public class UserDeleter
    {
        public static async Task<string> DeleteUser(string id)
        {
            string response = "";
            try
            {
                using (var db = new AppDbContext())
                {
                    db.Users.Remove(await db.Users.FindAsync(id));
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
