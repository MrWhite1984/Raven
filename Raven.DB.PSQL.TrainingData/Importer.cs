using Raven.DB.PSQL.TrainingData.Entity;

namespace Raven.DB.PSQL.TrainingData
{
    public class Importer
    {
        public static async Task<string> AddDataToTrainingDb(Post post, List<string> tagsNames)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var addedPost = db.Posts.Add(post).Entity;
                    await db.SaveChangesAsync();

                    foreach (var tagName in tagsNames)
                    {
                        var dbTag = db.Tags.Where(o=>o.Name == tagName).FirstOrDefault();
                        if (dbTag==null)
                        {
                            dbTag = db.Tags.Add(new Tag() { Name = tagName }).Entity;
                            await db.SaveChangesAsync();
                        }
                        db.PostTags.Add(new PostTags() { PostId = addedPost.Id, TagId = dbTag.Id});
                        await db.SaveChangesAsync();
                    }
                    
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        } 
    }
}
