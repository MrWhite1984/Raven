using Raven.DB.PSQL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.Logger
{
    public class LogsHandler
    {
        public static async Task ImportLogsAsync(List<Logs> logs)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    db.Logs.AddRange(logs);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
