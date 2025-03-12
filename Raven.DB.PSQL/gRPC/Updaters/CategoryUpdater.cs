using Raven.DB.PSQL.Entity;

namespace Raven.DB.PSQL.gRPC.Updaters
{
    public class CategoryUpdater
    {
        public static async Task<(string, Categories)> UpdateCategory(Categories category)
        {
            (string, Categories) response = new();
            try
            {
                using (var db = new AppDbContext())
                {
                    response.Item2 = db.Categories.Update(category).Entity;
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
