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
    }
}
