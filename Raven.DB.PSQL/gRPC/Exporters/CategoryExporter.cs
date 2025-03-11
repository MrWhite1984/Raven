using Microsoft.EntityFrameworkCore;
using Raven.DB.PSQL.Entity;

namespace Raven.DB.PSQL.gRPC.Exporters
{
    public class CategoryExporter
    {
        public static async Task<(List<Categories>, string)> GetCategoriesList()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    List<Categories> categories = await db.Categories.ToListAsync();

                    return (categories, "OK");
                }
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        public static async Task<(Categories, string)> GetCategory(int id)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    Categories category = await db.Categories.FindAsync(id);

                    return (category, "OK");
                }
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
    }
}
