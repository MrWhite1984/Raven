using Microsoft.EntityFrameworkCore;
using Raven.DB.PSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.gRPC.Exporters
{
    public class CommentExporter
    {
        public static async Task<(Comments, string)> GetComment(Guid id)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    Comments comment = await db.Comments
                        .Include(o => o.User)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    return (comment, "OK");
                }
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
    }
}
