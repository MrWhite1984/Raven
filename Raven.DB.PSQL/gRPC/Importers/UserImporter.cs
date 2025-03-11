using Raven.DB.PSQL.Entity;

namespace Raven.DB.PSQL.gRPC.Importers
{
    public class UserImporter
    {
        public static async Task<(string, Users)> CreateUser(Users user)
        {
            (string, Users) response = new();
            try
            {
                using (var db = new AppDbContext())
                {
                    response.Item2 = db.Users.Add(user).Entity;
                    await db.SaveChangesAsync();
                    response.Item1 = $"Пользователь {response.Item2.Id} добавлен в базу данных";
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
