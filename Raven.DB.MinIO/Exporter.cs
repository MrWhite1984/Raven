using Minio.DataModel.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.MinIO
{
    public class Exporter
    {
        public async static Task<(string, byte[])> GetCategoryImage(Guid guid)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                var getObjectArgs = new GetObjectArgs()
                    .WithBucket("category-images")
                    .WithObject(guid.ToString())
                    .WithCallbackStream(
                        async (stream) =>
                        {
                            await stream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;
                        }
                    );
                await new MinioContext().minioClient.GetObjectAsync(getObjectArgs);
                return ("OK", memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                return(ex.Message, null);
            }
        }

        public async static Task<(string, byte[])> GetPostImage(Guid guid)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                var getObjectArgs = new GetObjectArgs()
                    .WithBucket("post-images")
                    .WithObject(guid.ToString())
                    .WithCallbackStream(
                        async (stream) =>
                        {
                            await stream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;
                        }
                    );
                await new MinioContext().minioClient.GetObjectAsync(getObjectArgs);
                return ("OK", memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                return (ex.Message, null);
            }
        }

        public async static Task<(string, byte[])> GetPostVideos(Guid guid)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                var getObjectArgs = new GetObjectArgs()
                    .WithBucket("post-videos")
                    .WithObject(guid.ToString())
                    .WithCallbackStream(
                        async (stream) =>
                        {
                            await stream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;
                        }
                    );
                await new MinioContext().minioClient.GetObjectAsync(getObjectArgs);
                return ("OK", memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                return (ex.Message, null);
            }
        }

        public async static Task<(string, byte[])> GetCommentImage(Guid guid)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                var getObjectArgs = new GetObjectArgs()
                    .WithBucket("comment-images")
                    .WithObject(guid.ToString())
                    .WithCallbackStream(
                        async (stream) =>
                        {
                            await stream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;
                        }
                    );
                await new MinioContext().minioClient.GetObjectAsync(getObjectArgs);
                return ("OK", memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                return (ex.Message, null);
            }
        }

        public async static Task<(string, byte[])> GetCommentVideos(Guid guid)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                var getObjectArgs = new GetObjectArgs()
                    .WithBucket("comment-videos")
                    .WithObject(guid.ToString())
                    .WithCallbackStream(
                        async (stream) =>
                        {
                            await stream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;
                        }
                    );
                await new MinioContext().minioClient.GetObjectAsync(getObjectArgs);
                return ("OK", memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                return (ex.Message, null);
            }
        }
    }
}
