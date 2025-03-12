namespace Raven.DB.Neo4j.Importers
{
    public class Neo4jPostImporter
    {
        public async static Task<string> AddNewPost(string postId)
        {
            try
            {
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["AddNewPost"], new { postId = postId });
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
