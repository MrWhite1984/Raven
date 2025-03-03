using Raven.DB.PSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.gRPC.Importers
{
    public class CommentImporter
    {
        public async static Task<(string, Comments)> CreateComment(Comments comment)
        {
            (string, Comments) response = new();
            try
            {
                using (var db = new AppDbContext())
                {
                    response.Item2 = db.Comments.Add(comment).Entity;
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
