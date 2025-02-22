using Minio.DataModel.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.MinIO
{
    public class Importer
    {
        public async static Task<(string, Guid?)> AddNewCategoryImage(byte[] imageData)
        {
            try
            {
                using var memoryStream = new MemoryStream(imageData);
                var imageName = Guid.NewGuid();
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket("category-images")
                    .WithObject(imageName.ToString())
                    .WithStreamData(memoryStream)
                    .WithObjectSize(memoryStream.Length)
                    .WithContentType("image/jpeg");
                var minioContext = new MinioContext();
                await minioContext.minioClient.PutObjectAsync(putObjectArgs);
                return ("OK", imageName);
            }
            catch (Exception ex)
            {
                return (ex.Message, null);
            }
        }
    }
}
