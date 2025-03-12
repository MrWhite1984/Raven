namespace Raven.DB.Neo4j.Importers
{
    public class Neo4jCommentImporter
    {
        public async static Task<string> AddNewCommentToPost(string postId, string commentId)
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

        public async static Task<string> AddNewCommentToComment(string commentToId, string commentId)
        {
            try
            {
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["AddNewCommentToComment"],
                        new
                        {
                            commentId = commentId,
                            commentToId = commentToId
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
