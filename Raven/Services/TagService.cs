using Grpc.Core;
using Raven.DB.PSQL.Entity;
using Raven.DB.PSQL.gRPC.Deleters;
using Raven.DB.PSQL.gRPC.Exporters;
using Raven.DB.PSQL.gRPC.Importers;
using Raven.DB.PSQL.gRPC.Updaters;
using Raven.Logger;

namespace Raven.Services
{
    public class TagService : TagHandler.TagHandlerBase
    {
        public override Task<GetTagsResponse> GetTags(GetTagsRequest request, ServerCallContext context)
        {
            GetTagsResponse response = new GetTagsResponse();
            var dbResponse = TagExporter.GetTagsList();
            if (dbResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item2);
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
                Logger.Logger.Log(LogLevel.Information, $"Выполнен запрос на получение тегов");
                response.Message = dbResponse.Result.Item2;
            }
            return Task.FromResult(response);
        }

        public override Task<CreateTagResponse> CreateTag(CreateTagRequest request, ServerCallContext context)
        {
            CreateTagResponse response = new CreateTagResponse();
            var dbResponse = TagImporter.CreateTag(new Tags()
            {
                Name = request.Name
            });
            if (dbResponse.Result.Item2 == null)
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item1);
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
                Logger.Logger.Log(LogLevel.Information, $"Тег {response.TagMessage.Name} добавлен в базу данных");
                response.Code = 200;
                response.Message += dbResponse.Result.Item1;
            }
            return Task.FromResult(response);
        }

        public override Task<UpdateTagResponse> UpdateTag(UpdateTagRequest request, ServerCallContext context)
        {
            UpdateTagResponse response = new UpdateTagResponse();
            if(request.Id == 0)
            {
                Logger.Logger.Log(LogLevel.Error, $"Id было 0");
                response.Code = 500;
                response.Message = "Id было 0";
                return Task.FromResult(response);
            }
            var dbResponse = TagUpdater.UpdateTag(new Tags()
            {
                Id = (int)request.Id,
                Name = request.Name
            });
            if (dbResponse.Result.Item2 == null)
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item1);
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
                Logger.Logger.Log(LogLevel.Information, $"Тег {response.TagMessage.Name} обновлен");
                response.Code = 200;
                response.Message += dbResponse.Result.Item1;
            }
            return Task.FromResult(response);
        }

        public override Task<DeleteTagResponse> DeleteTag(DeleteTagRequest request, ServerCallContext context)
        {
            DeleteTagResponse response = new DeleteTagResponse();
            if (request.Id == 0)
            {
                Logger.Logger.Log(LogLevel.Error, $"Id было 0");
                response.Code = 500;
                response.Message = "Id было 0";
                return Task.FromResult(response);
            }
            var dbResponse = TagDeleter.DeleteTag((int)request.Id);
            if (dbResponse.Result != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result);
                response.Code = 500;
                response.Message = dbResponse.Result;
            }
            else
            {
                Logger.Logger.Log(LogLevel.Information, $"Тег с Id = {request.Id} удален из базы данных");
                response.Code = 200;
                response.Message = dbResponse.Result;
            }
            return Task.FromResult(response);
        }
    }
}
