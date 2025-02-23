using Microsoft.EntityFrameworkCore;
using Raven.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.gRPC.Deleters
{
    public class TagDeleter
    {
        public static async Task<string> DeleteTag(int id)
        {
            string response = "";
            try
            {
                using (var db = new AppDbContext())
                {
                    db.Tags.Remove(await db.Tags.FindAsync(id));
                    await db.SaveChangesAsync();
                    response = $"OK";
                    return response;
                }
            }
            catch (Exception ex)
            {
                return (ex.Message);
            }
        }
    }
}
