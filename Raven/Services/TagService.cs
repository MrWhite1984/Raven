using Grpc.Core;
using Raven.DB.PSQL.Entity;

namespace Raven.Services
{
    public class TagService : TagHandler.TagHandlerBase
    {
        public override Task<GetTagsResponse> GetTags(GetTagsRequest request, ServerCallContext context)
        {
            GetTagsResponse response = new GetTagsResponse();
            var dbResponse = DB.PSQL.gRPC.Exporter.GetTagsList();
            if (dbResponse.IsCanceled)
            {
                response.Entities.Add(new List<TagMessage>());
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
            }
            else if (dbResponse.Result.Item2 != "OK")
            {
                response.Entities.Add(new List<TagMessage>());
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
            }
            else
            {
                foreach (var tag in dbResponse.Result.Item1)
                    response.Entities
                        .Add(new TagMessage()
                        {
                            Id = (uint)tag.Id,
                            Name = tag.Name
                        });


                response.Code = 200;
                response.Message = dbResponse.Result.Item2;
            }
            return Task.FromResult(response);
        }

        public override Task<CreateTagResponse> CreateTag(CreateTagRequest request, ServerCallContext context)
        {
            CreateTagResponse response = new CreateTagResponse();
            var dbResponse = DB.PSQL.gRPC.Importer.CreateTag(new Tags()
            {
                Name = request.Name
            });
            if (dbResponse.IsCanceled)
            {
                response.TagMessage = null;
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
            }
            else if (dbResponse.Result.Item2 == null)
            {
                response.TagMessage = null;
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
            }
            else
            {
                response.TagMessage = new TagMessage()
                {
                    Id = (uint)dbResponse.Result.Item2.Id,
                    Name = dbResponse.Result.Item2.Name
                };
                response.Code = 200;
                response.Message += dbResponse.Result.Item1;
            }
            return Task.FromResult(response);
        }
    }
}
