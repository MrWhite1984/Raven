using Microsoft.EntityFrameworkCore;
using Raven.DB.PSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.gRPC.Exporters
{
    public class CommentContentExporter
    {
        public static async Task<(List<CommentContent>, string)> GetContentComment(Guid commentId)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var commentContents = db.CommentContent
                        .Where(o => o.CommentId == commentId)
                        .ToListAsync();

                    return (commentContents.Result, "OK");
                }
            }
            catch (Exception ex)
            {
                return (new List<CommentContent>(), ex.Message);
            }
        }
    }
}
