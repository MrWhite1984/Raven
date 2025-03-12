using Raven.DB.PSQL.Entity;

namespace Raven.DB.PSQL.gRPC.Importers
{
    public class TagsPostImporter
    {
        public static async Task<(string, TagsPosts)> CreateTagsPost(TagsPosts tagsPosts)
        {
            (string, TagsPosts) response = new();
            try
            {
                using (var db = new AppDbContext())
                {
                    response.Item2 = db.TagsPosts.Add(tagsPosts).Entity;
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
