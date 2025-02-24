using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Raven.DB.MinIO;
using Raven.Models;
using Google.Protobuf;
using Microsoft.AspNetCore.Identity.Data;
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
                    if (!category.ImageFile.Equals(Guid.Empty))
                    {
                        response.Entities
                        .Add(new CategoryMessage()
                        {
                            Id = (uint)category.Id,
                            Title = category.Title,
                            ImageFile = category.ImageFile.ToString(),
                            PostCount = (uint)category.PostCount,
                            Image = ByteString.CopyFrom(Exporter.GetCategoryImage(category.ImageFile).Result.Item2)
                        });
                    }
                    else
                    {
                        response.Entities
                        .Add(new CategoryMessage()
                        {
                            Id = (uint)category.Id,
                            Title = category.Title,
                            PostCount = (uint)category.PostCount
                        });
                    }

                    
                response.Code = 200;
                response.Message = dbResponse.Result.Item2;
            }
            return Task.FromResult(response);
        }

        public override Task<CreateCategoryResponse> CreateCategory(CreateCategoryRequest request, ServerCallContext context)
        {
            CreateCategoryResponse response = new CreateCategoryResponse();

            if(request.Image.Length != 0)
            {
                var minioResponse = Importer.AddNewCategoryImage(request.Image.ToByteArray());
                if (minioResponse.IsCanceled)
                {
                    response.CategoryMessage = null;
                    response.Code = 500;
                    response.Message = minioResponse.Result.Item1;
                }
                else if (minioResponse.Result.Item2.Equals(null))
                {
                    response.CategoryMessage = null;
                    response.Code = 500;
                    response.Message = minioResponse.Result.Item1;
                }
                else
                {
                    var dbResponse = CategoryImporter.CreateCategory(new Categories() 
                    { 
                        Title = request.Title, 
                        ImageFile = minioResponse.Result.Item2 ?? Guid.Empty 
                    });
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
                            Id = (uint)dbResponse.Result.Item2.Id,
                            Title = dbResponse.Result.Item2.Title,
                            ImageFile = dbResponse.Result.Item2.ImageFile.ToString(),
                            PostCount = (uint)dbResponse.Result.Item2.PostCount
                        };
                        response.Code = 200;
                        response.Message += dbResponse.Result.Item1;
                    }
                }
            }
            else
            {
                var dbResponse = CategoryImporter.CreateCategory(new Categories() { Title = request.Title });
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
                        Id = (uint)dbResponse.Result.Item2.Id,
                        Title = dbResponse.Result.Item2.Title,
                        ImageFile = dbResponse.Result.Item2.ImageFile.ToString(),
                        PostCount = (uint)dbResponse.Result.Item2.PostCount
                    };
                    response.Code = 200;
                    response.Message += dbResponse.Result.Item1;
                }
            }
                        
            return Task.FromResult(response);
        }

        public override Task<UpdateCategoryResponse> UpdateCategory(UpdateCategoryRequest request, ServerCallContext context)
        {
            UpdateCategoryResponse response = new UpdateCategoryResponse();
            if(request.Id == 0)
            {
                response.CategoryMessage = null;
                response.Code = 500;
                response.Message = "Id было 0";
                return Task.FromResult(response);
            }
            if (request.Image.Length != 0)
            {
                var minioResponse = Importer.AddNewCategoryImage(request.Image.ToByteArray());
                if (minioResponse.IsCanceled)
                {
                    response.CategoryMessage = null;
                    response.Code = 500;
                    response.Message = minioResponse.Result.Item1;
                }
                else if (minioResponse.Result.Item2.Equals(null))
                {
                    response.CategoryMessage = null;
                    response.Code = 500;
                    response.Message = minioResponse.Result.Item1;
                }
                else
                {
                    var dbResponse = CategoryUpdater.UpdateCategory(new Categories()
                    {
                        Id = (int)request.Id,
                        Title = request.Title,
                        ImageFile = minioResponse.Result.Item2 ?? Guid.Empty
                    });
                    if (dbResponse.IsCanceled)
                    {
                        response.CategoryMessage = null;
                        response.Code = 500;
                        response.Message = dbResponse.Result.Item1;
                    }
                    else if (dbResponse.Result.Item1 != "OK")
                    {
                        response.CategoryMessage = null;
                        response.Code = 500;
                        response.Message = dbResponse.Result.Item1;
                    }
                    else
                    {
                        response.CategoryMessage = new CategoryMessage()
                        {
                            Id = (uint)dbResponse.Result.Item2.Id,
                            Title = dbResponse.Result.Item2.Title,
                            ImageFile = dbResponse.Result.Item2.ImageFile.ToString(),
                            PostCount = (uint)dbResponse.Result.Item2.PostCount
                        };
                        response.Code = 200;
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
                        Id = (uint)dbResponse.Result.Item2.Id,
                        Title = dbResponse.Result.Item2.Title,
                        ImageFile = dbResponse.Result.Item2.ImageFile.ToString(),
                        PostCount = (uint)dbResponse.Result.Item2.PostCount
                    };
                    response.Code = 200;
                    response.Message += dbResponse.Result.Item1;
                }
            }

            return Task.FromResult(response);
        }
    }
}
