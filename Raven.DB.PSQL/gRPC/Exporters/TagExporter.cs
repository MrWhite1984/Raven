using Microsoft.EntityFrameworkCore;
using Raven.DB.PSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.gRPC.Exporters
{
    public class TagExporter
    {
        public static async Task<(List<Tags>, string)> GetTagsList()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    List<Tags> tags = await db.Tags.ToListAsync();
                    return (tags, "OK");
                }
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        public static async Task<(Tags, string)> GetTag(int id)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    Tags tags = await db.Tags.FindAsync(id);

                    return (tags, "OK");
                }
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        public static async Task<(List<Tags>, string)> GetTagsByNames(List<string> names)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    List<Tags> tags = await db.Tags.Where(o=>names.Contains(o.Name)).ToListAsync();

                    return (tags, "OK");
                }
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
    }
}
