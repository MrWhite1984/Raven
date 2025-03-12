using Raven.DB.PSQL.Entity;

namespace Raven.DB.PSQL.gRPC.Importers
{
    public class CategoryImporter
    {
        public static async Task<(string, Categories)> CreateCategory(Categories category)
        {
            (string, Categories) response = new();
            try
            {
                using (var db = new AppDbContext())
                {
                    response.Item2 = db.Categories.Add(category).Entity;
                    await db.SaveChangesAsync();
                    response.Item1 = $"Категория {response.Item2.Title} добавлена в базу данных";
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
