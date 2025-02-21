using Minio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.MinIO
{
    public class MinioContext
    {
        private MinioClient _minioClient;
        public MinioContext()
        {
            _minioClient = (MinioClient)new MinioClient()
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
                var bucketExists = await _minioClient.BucketExistsAsync(exArgs);
                if (!bucketExists)
                {
                    await _minioClient.MakeBucketAsync(new Minio.DataModel.Args.MakeBucketArgs().WithBucket(bucketName));
                }
            }
        }


        
    }
}
