using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.Neo4j.Deleters
{
    public class Neo4jRecommendedRelationshipDeleter
    {
        public async static Task<string> DeleteRelationship(string userId, string postId)
        {
            try
            {
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["DeleteRecommendedRelationship"], 
                        new { 
                            userId = userId,
                            postId = postId 
                        });
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
