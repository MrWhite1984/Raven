using Microsoft.EntityFrameworkCore;
using Raven.DB.PSQL.Entity;

namespace Raven.DB.PSQL.gRPC.Exporters
{
    public class PostExporter
    {
        public static async Task<(Posts, string)> GetPost(Guid id)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    Posts post = await db.Posts
                        .Include(o => o.TagsPosts)
                        .ThenInclude(o => o.Tag)
                        .Include(o => o.PostContents)
                        .Include(o => o.User)
                        .Include(o => o.CategoryPost)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    return (post, "OK");
                }
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        public static async Task<(List<Posts>, string, DateTime)> GetPosts(DateTime cursor, int pageSize)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var posts = await db.Posts
                        .Include(o => o.TagsPosts)
                        .ThenInclude(o => o.Tag)
                        .Include(o => o.PostContents)
                        .Include(o => o.User)
                        .Include(o => o.CategoryPost)
                        .OrderByDescending(p => p.CreatedAt)
                        .Where(o => o.CreatedAt <= cursor)
                        .Take(pageSize+1)
                        .ToListAsync();
                    return (posts.Take(pageSize).ToList(), "OK", posts.Last().CreatedAt);
                }
            }
            catch(Exception ex)
            {
                return (new List<Posts>(), ex.Message, DateTime.MinValue);
            }
        }

        public static async Task<(List<Posts>, string)> GetPostsByIdsList(List<string> ids)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var posts = await db.Posts
                        .Include(o => o.TagsPosts)
                        .ThenInclude(o => o.Tag)
                        .Include(o => o.PostContents)
                        .Include(o => o.User)
                        .Include(o => o.CategoryPost)
                        .OrderByDescending(p => p.CreatedAt)
                        .Where(o => ids.Contains(o.Id.ToString()))
                        .ToListAsync();
                    return (posts, "OK");
                }
            }
            catch (Exception ex)
            {
                return (new List<Posts>(), ex.Message);
            }
        }

        public static async Task<(List<Posts>, string, DateTime)> GetPostsByUser
            (DateTime cursor, int pageSize, string userId)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var posts = await db.Posts
                        .Include(o => o.TagsPosts)
                        .ThenInclude(o => o.Tag)
                        .Include(o => o.PostContents)
                        .Include(o => o.User)
                        .Include(o => o.CategoryPost)
                        .OrderByDescending(p => p.CreatedAt)
                        .Where(o => o.CreatedAt <= cursor && o.AuthorId == userId)
                        .Take(pageSize + 1)
                        .ToListAsync();
                    return (posts.Take(pageSize).ToList(), "OK", posts.Last().CreatedAt);
                }
            }
            catch (Exception ex)
            {
                return (new List<Posts>(), ex.Message, DateTime.MinValue);
            }
        }

        public static async Task<(List<Posts>, string, DateTime)> GetPostsByCategoryId
            (DateTime cursor, int pageSize, int categoryId)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var posts = await db.Posts
                        .Include(o => o.TagsPosts)
                        .ThenInclude(o => o.Tag)
                        .Include(o => o.PostContents)
                        .Include(o => o.User)
                        .Include(o => o.CategoryPost)
                        .OrderByDescending(p => p.CreatedAt)
                        .Where(o => o.CreatedAt <= cursor && o.CategoryId == categoryId)
                        .Take(pageSize + 1)
                        .ToListAsync();
                    return (posts.Take(pageSize).ToList(), "OK", posts.Last().CreatedAt);
                }
            }
            catch (Exception ex)
            {
                return (new List<Posts>(), ex.Message, DateTime.MinValue);
            }
        }

        public static async Task<(List<Posts>, string, DateTime)> GetPostsByTagsId
            (DateTime cursor, int pageSize, List<int> tagsId)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var postsId = await db.TagsPosts
                        .Where(o => tagsId.Contains(o.TagId))
                        .Select(o => o.PostId)
                        .ToListAsync();
                    var posts = await db.Posts
                        .Include(o => o.TagsPosts)
                        .ThenInclude(o => o.Tag)
                        .Include(o => o.PostContents)
                        .Include(o => o.User)
                        .Include(o => o.CategoryPost)
                        .OrderByDescending(p => p.CreatedAt)
                        .Where(o => o.CreatedAt <= cursor && postsId.Contains(o.Id))
                        .Take(pageSize + 1)
                        .ToListAsync();
                    return (posts.Take(pageSize).ToList(), "OK", posts.Last().CreatedAt);
                }
            }
            catch (Exception ex)
            {
                return (new List<Posts>(), ex.Message, DateTime.MinValue);
            }
        }
    }
}
