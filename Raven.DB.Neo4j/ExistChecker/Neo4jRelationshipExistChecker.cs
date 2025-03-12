using Neo4j.Driver;

namespace Raven.DB.Neo4j.ExistChecker
{
    public class Neo4jRelationshipExistChecker
    {
        public async static Task<(string, bool)> CheckExistPostLikeRelationship(string userId, string postId)
        {
            try
            {
                List<IRelationship> nodes = new List<IRelationship>();
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["CheckExistPostLikeRelationship"], new
                    {
                        userId = userId,
                        postId = postId
                    });
                    if (await result.FetchAsync())
                    {
                        return ("OK", result.Current["Exist"].As<bool>());
                    }
                    else  { return ("OK", false); }
                }
            }
            catch (Exception ex)
            {
                return (ex.Message, false);
            }
        }

        public async static Task<(string, bool)> CheckExistPostViewRelationship(string userId, string postId)
        {
            try
            {
                List<IRelationship> nodes = new List<IRelationship>();
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["CheckExistPostViewRelationship"], new
                    {
                        userId = userId,
                        postId = postId
                    });
                    if (await result.FetchAsync())
                    {
                        return ("OK", result.Current["Exist"].As<bool>());
                    }
                    else { return ("OK", false); }
                }
            }
            catch (Exception ex)
            {
                return (ex.Message, false);
            }
        }

        public async static Task<(string, bool)> CheckExistPostBookmarkRelationship(string userId, string postId)
        {
            try
            {
                List<IRelationship> nodes = new List<IRelationship>();
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["CheckExistPostBookmarkRelationship"], new
                    {
                        userId = userId,
                        postId = postId
                    });
                    if (await result.FetchAsync())
                    {
                        return ("OK", result.Current["Exist"].As<bool>());
                    }
                    else { return ("OK", false); }
                }
            }
            catch (Exception ex)
            {
                return (ex.Message, false);
            }
        }
    }
}
