using Raven.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.gRPC
{
    public class Importer
    {
        public static async Task<(string, Categories)> CreateCategory(Categories category)
        {
            (string, Categories) response = new();
            try
            {
                using(var db = new AppDbContext())
                {
                    response.Item2 = db.Categories.Add(category).Entity;
                    await db.SaveChangesAsync();
                    response.Item1 = $"Категория {response.Item2.Title} добавлена в базу данных";
                }
                return response;
            }
            catch (Exception ex)
            {
                return(ex.Message, null);
            }
        }
    }
}
