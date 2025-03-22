using Grpc.Core;
using Raven.DB.MinIO;
using Raven.DB.PSQL.Entity;
using Raven.DB.PSQL.gRPC.Exporters;
using Raven.DB.PSQL.gRPC.Importers;
using Raven.DB.PSQL.gRPC.Updaters;

namespace Raven.Services
{
    public class CategoryService : CategoryHandler.CategoryHandlerBase
    {
        public override Task<GetCategoriesResponse> GetCategories(GetCategoriesRequest request, ServerCallContext context)
        {
            GetCategoriesResponse response = new GetCategoriesResponse();
            var dbResponse = CategoryExporter.GetCategoriesList();
            if (dbResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item2);
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
                return Task.FromResult(response);
            }
            foreach (var category in dbResponse.Result.Item1)
                response.Entities
                    .Add(CreateCategoryMessage(category));

            Logger.Logger
                .Log(LogLevel.Information, $"Категории ({response.Entities.Count}) получены из базы данных");
            response.Code = 200;
            response.Message = dbResponse.Result.Item2;
            return Task.FromResult(response);
        }

        public override Task<CreateCategoryResponse> CreateCategory(CreateCategoryRequest request, ServerCallContext context)
        {
            CreateCategoryResponse response = new CreateCategoryResponse();

            if (request.Image.Length != 0)
            {
                var minioResponse = Importer.AddNewCategoryImage(request.Image.ToByteArray());
                if (minioResponse.Result.Item2.Equals(null))
                {
                    Logger.Logger.Log(LogLevel.Error, minioResponse.Result.Item1);
                    response.Code = 500;
                    response.Message = minioResponse.Result.Item1;
                    return Task.FromResult(response);
                }
                var dbResponse = CategoryImporter.CreateCategory(new Categories()
                {
                    Title = request.Title,
                    ImageFile = minioResponse.Result.Item2 ?? Guid.Empty
                });
                if (dbResponse.Result.Item2 == null)
                {
                    Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item1);
                    response.Code = 500;
                    response.Message = dbResponse.Result.Item1;
                    return Task.FromResult(response);
                }
                else
                {
                    response.CategoryMessage = CreateCategoryMessage(dbResponse.Result.Item2);
                    response.Message += dbResponse.Result.Item1;
                }
            }
            else
            {
                var dbResponse = CategoryImporter.CreateCategory(new Categories() { Title = request.Title });
                if (dbResponse.Result.Item2 == null)
                {
                    Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item1);
                    response.Code = 500;
                    response.Message = dbResponse.Result.Item1;
                    return Task.FromResult(response);
                }
                else
                {
                    response.CategoryMessage = CreateCategoryMessage(dbResponse.Result.Item2);
                    response.Message += dbResponse.Result.Item1;
                }
            }
            Logger.Logger
                .Log(
                    LogLevel.Information,
                    $"Категория {response.CategoryMessage.Title} добавлена в базу данных"
                    );
            response.Code = 200;
            return Task.FromResult(response);
        }

        public override Task<UpdateCategoryResponse> UpdateCategory(UpdateCategoryRequest request, ServerCallContext context)
        {
            UpdateCategoryResponse response = new UpdateCategoryResponse();
            if(request.Id == 0)
            {
                Logger.Logger.Log(LogLevel.Error, "Id было 0");
                response.Code = 500;
                response.Message = "Id было 0";
                return Task.FromResult(response);
            }
            if (request.Image.Length != 0)
            {
                var minioResponse = Importer.AddNewCategoryImage(request.Image.ToByteArray());
                if (minioResponse.Result.Item2.Equals(null))
                {
                    Logger.Logger.Log(LogLevel.Error, minioResponse.Result.Item1);
                    response.Code = 500;
                    response.Message = minioResponse.Result.Item1;
                    return Task.FromResult(response);
                }
                else
                {
                    var dbResponse = CategoryUpdater.UpdateCategory(new Categories()
                    {
                        Id = (int)request.Id,
                        Title = request.Title,
                        ImageFile = minioResponse.Result.Item2 ?? Guid.Empty
                    });
                    if (dbResponse.Result.Item1 != "OK")
                    {
                        Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item1);
                        response.Code = 500;
                        response.Message = dbResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    else
                    {
                        response.CategoryMessage = CreateCategoryMessage(dbResponse.Result.Item2);
                        response.Message += dbResponse.Result.Item1;
                    }
                }
            }
            else
            {
                var dbResponse = CategoryUpdater.UpdateCategory(new Categories() 
                { 
                    Id = (int)request.Id,
                    Title = request.Title 
                });
                if (dbResponse.Result.Item2 == null)
                {
                    Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item1);
                    response.Code = 500;
                    response.Message = dbResponse.Result.Item1;
                    return Task.FromResult(response);
                }
                else
                {
                    response.CategoryMessage = CreateCategoryMessage(dbResponse.Result.Item2);
                    response.Message += dbResponse.Result.Item1;
                }
            }
            Logger.Logger.Log(LogLevel.Information, $"Категория {response.CategoryMessage.Id} обновлена");
            response.Code = 200;
            return Task.FromResult(response);
        }

        public static CategoryMessage CreateCategoryMessage(Categories category)
        {
            var response = new CategoryMessage()
            {
                Id = (uint)category.Id,
                Title = category.Title,
                ImageFile = category.ImageFile.ToString()
            };
            return response;
        }
    }
}
