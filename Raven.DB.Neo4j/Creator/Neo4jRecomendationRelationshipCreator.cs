namespace Raven.DB.Neo4j.Creator
{
    public class Neo4jRecomendationRelationshipCreator
    {
        public async static Task<string> CreateNewRecomendations(string userId)
        {
            try
            {
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["AddNewRecomendations"], new { userId = userId });
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
