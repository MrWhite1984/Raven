using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.Neo4j.Exporters
{
    public class Neo4jRelationshipExporter
    {
        public async static Task<string> GetPostLikeRelationship(string userId, string postId)
        {
            try
            {
                List<IRelationship> nodes = new List<IRelationship>();
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["GetPostLikeRelationship"], new
                    {
                        userId = userId,
                        postId = postId
                    });
                    await foreach (var record in result)
                    {
                        nodes.Add(record["r"].As<IRelationship>());
                    }
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
