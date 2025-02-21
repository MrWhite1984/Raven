using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Raven.DB.PSQL.gRPC;
using Raven.Entity;
using Raven.Models;

namespace Raven.Services
{
    public class ContentService : ContentHandler.ContentHandlerBase
    {
        public override Task<GetCategoriesResponse> GetCategories(GetCategoriesRequest request, ServerCallContext context)
        {
            GetCategoriesResponse response = new GetCategoriesResponse();
            var dbResponse = Exporter.GetCategoriesList();
            if (dbResponse.IsCanceled)
            {
                response.Entities.Add(new List<CategoryMessage>());
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
            }
            else if (dbResponse.Result.Item2 != "OK")
            {
                response.Entities.Add(new List<CategoryMessage>());
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
            }
            else
            {
                foreach (var category in dbResponse.Result.Item1)
                    response.Entities
                        .Add(new CategoryMessage() {
                            Id = (uint)category.Id,
                            Title = category.Title,
                            ImageFile = category.ImageFile.ToString(),
                            PostCount = (uint)category.PostCount});
                response.Code = 200;
                response.Message = dbResponse.Result.Item2;
            }
            return Task.FromResult(response);
        }

        public override Task<CreateCategoryResponse> CreateCategory(CreateCategoryRequest request, ServerCallContext context)
        {
            CreateCategoryResponse response = new CreateCategoryResponse();
            var dbResponse = Importer.CreateCategory(new Categories() { Title = request.Title });
            if (dbResponse.IsCanceled)
            {
                response.CategoryMessage = null;
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
            }
            else if (dbResponse.Result.Item2 == null)
            {
                response.CategoryMessage = null;
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
            }
            else
            {
                response.CategoryMessage = new CategoryMessage()
                { 
                    Id = dbResponse.Result.Item2.Id,
                    Title = dbResponse.Result.Item2.Title,
                    ImageFile = dbResponse.Result.Item2.ImageFile.ToString(),
                    PostCount = dbResponse.Result.Item2.PostCount
                };
                response.Code = 200;
                response.Message += dbResponse.Result.Item1;
            }
            return Task.FromResult(response);
        }
    }
}
