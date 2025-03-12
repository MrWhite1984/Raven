using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
                    db.Logs.AddRange(logs.Select(log => {
                        log.DateTime = log.DateTime.ToUniversalTime();
                        return log;
                    }).ToList());
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static Task<List<Logs>> GetLogs
            ()
        {
            var response = (new List<Logs>());
            try
            {
                using (var db = new AppDbContext())
                {
                    List<Logs> logs = db.Logs
                             .OrderByDescending(o => o.DateTime)
                             .Take(10000)
                             .ToList();
                    response = logs.ToList();
                }
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                return Task.FromResult(response);
            }
        }

        public static Task<(List<Logs>, DateTime)> GetLogs
            (DateTime cursor, DateTime startDate, DateTime endDate, int pageSize)
        {
            var response = (new List<Logs>(), DateTime.Now);
            try
            {
                using (var db = new AppDbContext())
                {
                    List<Logs> logs = db.Logs
                             .OrderByDescending(o => o.DateTime)
                             .ToList();
                    response.Item1 = logs.Take(pageSize).ToList();
                    response.Item2 = logs.Last().DateTime;
                }
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                return Task.FromResult(response);
            }
        }
    }
}
