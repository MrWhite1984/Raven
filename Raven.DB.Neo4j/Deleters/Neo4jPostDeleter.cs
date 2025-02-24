using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.Neo4j.Deleters
{
    public class Neo4jPostDeleter
    {
        public async static Task<string> DeletePost(string postId)
        {
            try
            {
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["DeletePost"], new { postId = postId });
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return (ex.Message);
            }
        }
    }
}
