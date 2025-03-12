using Microsoft.EntityFrameworkCore;
using Raven.DB.PSQL.Entity;

namespace Raven.DB.PSQL.gRPC.Exporters
{
    public class UserExporter
    {
        public static async Task<(List<Users>, string)> GetUsersList()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    List<Users> users = await db.Users.ToListAsync();

                    return (users, "OK");
                }
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
    }
}
