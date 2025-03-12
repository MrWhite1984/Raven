using Minio;

namespace Raven.DB.MinIO
{
    public class MinioContext
    {
        public MinioClient minioClient;
        public MinioContext()
        {
            minioClient = (MinioClient)new MinioClient()
                .WithEndpoint("minio", 9000)
                .WithCredentials("ravenMinIOaccess", "ravenMinIOsecret")
                .Build();
        }

        public async Task EnsureBucketsExist()
        {
            var bucketNames = new List<string> 
            { 
                "logs",
                "category-images",
                "post-images",
                "post-videos",
                "comment-images",
                "comment-videos" 
            };

            foreach (var bucketName in bucketNames)
            {
                var exArgs = new Minio.DataModel.Args.BucketExistsArgs().WithBucket(bucketName);
                var bucketExists = await minioClient.BucketExistsAsync(exArgs);
                if (!bucketExists)
                {
                    await minioClient.MakeBucketAsync(new Minio.DataModel.Args.MakeBucketArgs().WithBucket(bucketName));
                }
            }
        }


        
    }
}
