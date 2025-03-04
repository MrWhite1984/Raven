using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.Neo4j.Importers
{
    public class Neo4jRelationshipImporter
    {
        public async static Task<string> AddPostLikeRelationship(string userId, string postId)
        {
            try
            {
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["AddPostLikeRelationship"], new { 
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

        public async static Task<string> AddPostViewRelationship(string userId, string postId)
        {
            try
            {
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["AddPostViewRelationship"], new
                    {
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

        public async static Task<string> AddPostBookmarkRelationship(string userId, string postId)
        {
            try
            {
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["AddPostBookmarkRelationship"], new
                    {
                        userId = userId,
                        postId = postId,
                        bookmarkDate = DateTime.Now
                    });
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return (ex.Message);
            }
        }

        public async static Task<string> AddCommentLikeRelationship(string userId, string commentId)
        {
            try
            {
                using (var session = new Neo4jContext().driver.AsyncSession())
                {
                    var result = await session.RunAsync(Neo4jContext.CypherQuerries["AddCommentLikeRelationship"], new
                    {
                        userId = userId,
                        commentId = commentId
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
