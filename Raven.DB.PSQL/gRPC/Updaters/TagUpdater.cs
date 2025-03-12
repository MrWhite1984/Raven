using Raven.DB.PSQL.Entity;

namespace Raven.DB.PSQL.gRPC.Updaters
{
    public class TagUpdater
    {
        public static async Task<(string, Tags)> UpdateTag(Tags tag)
        {
            (string, Tags) response = new();
            try
            {
                using (var db = new AppDbContext())
                {
                    response.Item2 = db.Tags.Update(tag).Entity;
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
