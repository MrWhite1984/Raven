using Neo4j.Driver;

namespace Raven.DB.Neo4j.Exporters
{
    public class Neo4jCommentExporter
    {
        public async static Task<(string, List<string>)> GetCommentsToPost(string postId)
        {
            try
            {
                List<string> postIds = new List<string>();
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var response = await session.RunAsync(Neo4jContext.CypherQuerries["GetCommentsToPost"], new
                    {
                        postId = postId
                    });
                    var result = await response.ToListAsync();
                    foreach (var record in result)
                    {
                        postIds.Add(record["uniqueComments"].As<INode>().Properties["CommentId"].ToString());
                    }
                }

                return ("OK", postIds);
            }
            catch (Exception ex)
            {
                return (ex.Message, new List<string>());
            }
        }

        public async static Task<(string, List<string>)> GetCommentsToComment(string commentId)
        {
            try
            {
                List<string> postIds = new List<string>();
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var response = await session.RunAsync(Neo4jContext.CypherQuerries["GetCommentsToComment"], new
                    {
                        commentToId = commentId
                    });
                    var result = await response.ToListAsync();
                    foreach (var record in result)
                    {
                        postIds.Add(record["uniqueComments"].As<INode>().Properties["CommentId"].ToString());
                    }
                }

                return ("OK", postIds);
            }
            catch (Exception ex)
            {
                return (ex.Message, new List<string>());
            }
        }
    }
}
