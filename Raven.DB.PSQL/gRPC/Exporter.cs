using Microsoft.EntityFrameworkCore;
using Raven.DB.PSQL.Entity;
using Raven.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.gRPC
{
    public class Exporter
    {
        public static async Task<(List<Categories>, string)> GetCategoriesList()
        {
            try
            {
                using(var db = new AppDbContext())
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
    }
}
