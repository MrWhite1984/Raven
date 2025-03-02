using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.Neo4j.Exporters
{
    public class Neo4jPostExporter
    {
        public async static Task<(string, List<string>)> GetRecommendedPosts(string userId, int pageSize)
        {
            try
            {
                List<string> postIds = new List<string>();
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var response = await session.RunAsync(Neo4jContext.CypherQuerries["GetRecommendedPosts"], new
                    {
                        userId = userId,
                        pageSize = pageSize
                    });
                    var result = await response.ToListAsync();
                    foreach (var record in result)
                    {
                        postIds.Add(record["uniquePosts"].As<INode>().Properties["PostId"].ToString());
                    }
                }

                return ("OK", postIds);
            }
            catch (Exception ex)
            {
                return (ex.Message, new List<string>());
            }
        }
        public async static Task<(string, List<string>)> GetUnviewedPosts(string userId, int pageSize)
        {
            try
            {
                List<string> postIds = new List<string>();
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var response = await session.RunAsync(Neo4jContext.CypherQuerries["GetUnviewedPosts"], new
                    {
                        userId = userId,
                        pageSize = pageSize
                    });
                    var result = await response.ToListAsync();
                    foreach (var record in result)
                    {
                        postIds.Add(record["uniquePosts"].As<INode>().Properties["PostId"].ToString());
                    }
                }

                return ("OK", postIds);
            }
            catch (Exception ex)
            {
                return (ex.Message, new List<string>());
            }
        }

        public async static Task<(string, List<string>, DateTime)> GetBookmarkedPosts(string userId, int pageSize, DateTime cursor)
        {
            try
            {
                List<string> postIds = new List<string>();
                DateTime nextCursor;
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var response = await session.RunAsync(Neo4jContext.CypherQuerries["GetBookmarkedPosts"], new
                    {
                        userId = userId,
                        pageSize = pageSize,
                        cursor = cursor
                    });
                    var result = await response.ToListAsync();
                    foreach (var record in result)
                    {
                        postIds.Add(record["uniquePosts"].As<INode>().Properties["PostId"].ToString());
                    }
                    nextCursor = DateTime
                        .Parse(result.Last()["rels"]
                            .As<List<IRelationship>>()
                            .Last().Properties["BookmarkDate"]
                            .ToString())
                        .ToUniversalTime();
                }

                return ("OK", postIds, nextCursor);
            }
            catch (Exception ex)
            {
                return (ex.Message, new List<string>(), DateTime.MinValue);
            }
        }
    }
}
