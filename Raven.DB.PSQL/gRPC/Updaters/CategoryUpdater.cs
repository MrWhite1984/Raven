using Raven.DB.PSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
