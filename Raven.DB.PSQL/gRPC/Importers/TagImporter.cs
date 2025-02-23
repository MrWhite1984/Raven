using Raven.DB.PSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.gRPC.Importers
{
    public class TagImporter
    {
        public static async Task<(string, Tags)> CreateTag(Tags tag)
        {
            (string, Tags) response = new();
            try
            {
                using (var db = new AppDbContext())
                {
                    response.Item2 = db.Tags.Add(tag).Entity;
                    await db.SaveChangesAsync();
                    response.Item1 = $"Тег {response.Item2.Name} добавлена в базу данных";
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
