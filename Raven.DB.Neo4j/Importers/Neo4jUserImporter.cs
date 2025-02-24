using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.Neo4j.Importers
{
    public class Neo4jUserImporter
    {
        public async static Task<string> AddNewUser(string userId)
        {
            try
            {
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["AddNewUser"], new { userId = userId });
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return(ex.Message);
            }
        }
    }
}
