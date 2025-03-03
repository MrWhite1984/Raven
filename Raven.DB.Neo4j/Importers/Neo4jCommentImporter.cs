using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.Neo4j.Importers
{
    public class Neo4jCommentImporter
    {
        public async static Task<string> AddNewComment(string postId, string commentId)
        {
            try
            {
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["AddNewCommentToPost"],
                        new { 
                            commentId = commentId,
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
