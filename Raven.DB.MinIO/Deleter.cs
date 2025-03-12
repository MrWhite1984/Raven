using Minio.DataModel.Args;

namespace Raven.DB.MinIO
{
    public class Deleter
    {
        public async static Task<string> DeletePostImage(Guid id)
        {
            try
            {
                var args = new RemoveObjectArgs()
                    .WithBucket("post-images")
                    .WithObject(id.ToString());
                var minioContext = new MinioContext();
                await minioContext.minioClient.RemoveObjectAsync(args);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async static Task<string> DeletePostVideo(Guid id)
        {
            try
            {
                var args = new RemoveObjectArgs()
                    .WithBucket("post-videos")
                    .WithObject(id.ToString());
                var minioContext = new MinioContext();
                await minioContext.minioClient.RemoveObjectAsync(args);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
