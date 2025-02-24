using Raven.DB.PSQL.Entity;
using Raven.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.gRPC.Importers
{
    public class PostImporter
    {
        public static async Task<(string, Posts)> CreatePost(Posts post)
        {
            (string, Posts) response = new();
            try
            {
                using (var db = new AppDbContext())
                {
                    response.Item2 = db.Posts.Add(post).Entity;
                    await db.SaveChangesAsync();
                    response.Item1 = $"Пост {response.Item2.Title} добавлен в базу данных";
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
