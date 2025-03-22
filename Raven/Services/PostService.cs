using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Identity.Data;
using Newtonsoft.Json;
using Raven.DB.MinIO;
using Raven.DB.Neo4j.Creator;
using Raven.DB.Neo4j.Deleters;
using Raven.DB.Neo4j.ExistChecker;
using Raven.DB.Neo4j.Exporters;
using Raven.DB.Neo4j.Importers;
using Raven.DB.PSQL.Entity;
using Raven.DB.PSQL.Entity.@enum;
using Raven.DB.PSQL.gRPC.Deleters;
using Raven.DB.PSQL.gRPC.Exporters;
using Raven.DB.PSQL.gRPC.Importers;
using Raven.DB.PSQL.gRPC.Updaters;
using Raven.DB.Redis;
using Raven.JsonSettings;
using Raven.Models;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;

namespace Raven.Services
{
    public class PostService : PostHandler.PostHandlerBase
    {
        public override Task<CreatePostResponse> CreatePost(CreatePostRequest request, ServerCallContext context)
        {
            CreatePostResponse response = new CreatePostResponse();
            if(request.TagIds.Count == 0)
            {
                Logger.Logger.Log(LogLevel.Error, "TagIds было null");
                response.Code = 500;
                response.Message = "TagIds было null";
                return Task.FromResult(response);
            }
            var getCategoryResponse = CategoryExporter.GetCategory((int)request.CategoryId);
            if (getCategoryResponse.Result.Item1 == null)
            {
                Logger.Logger.Log(LogLevel.Error, getCategoryResponse.Result.Item2);
                response.Code = 500;
                response.Message = getCategoryResponse.Result.Item2;
                return Task.FromResult(response);
            }
            Task<(string, byte[])> getCategoryImageResponse = null;
            if (!getCategoryResponse.Result.Item1.ImageFile.Equals(Guid.Empty))
            {
                getCategoryImageResponse = Exporter.GetCategoryImage(getCategoryResponse.Result.Item1.ImageFile);
                if (getCategoryImageResponse.Result.Item1 != "OK")
                {
                    Logger.Logger.Log(LogLevel.Error, getCategoryImageResponse.Result.Item1);
                    response.Code = 500;
                    response.Message = getCategoryImageResponse.Result.Item1;
                    return Task.FromResult(response);
                }
            }
            List<(Guid, string, ContentType, byte[])> contentMetas = new List<(Guid, string, ContentType, byte[])>();
            foreach(var content in request.PostContentMessage)
            {
                if (content.ContentTypeEnum.ToString().Equals("Image"))
                {
                    var addContentResponse = Importer.AddNewPostImage(content.Content.ToByteArray());
                    if (addContentResponse.Result.Item2.Equals(null))
                    {
                        Logger.Logger.Log(LogLevel.Error, addContentResponse.Result.Item1);
                        response.Code = 500;
                        response.Message = addContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    contentMetas.Add((
                        addContentResponse.Result.Item2 ?? Guid.Empty,
                        content.Marker,
                        ContentType.Image,
                        content.Content.ToByteArray()));
                }
                else
                {
                    var addContentResponse = Importer.AddNewPostVideo(content.Content.ToByteArray());
                    if (addContentResponse.Result.Item2.Equals(null))
                    {
                        Logger.Logger.Log(LogLevel.Error, addContentResponse.Result.Item1);
                        response.Code = 500;
                        response.Message = addContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    contentMetas.Add((
                        addContentResponse.Result.Item2 ?? Guid.Empty,
                        content.Marker,
                        ContentType.Video,
                        content.Content.ToByteArray()));
                }
            }
            
            List<Tags> tags = new List<Tags>();
            List<TagMessage> tagMessages = new List<TagMessage>();
            foreach (var tagId in request.TagIds)
            {
                var getTagResponse = TagExporter.GetTag((int)tagId);
                if (getTagResponse.Result.Item1 == null)
                {
                    Logger.Logger.Log(LogLevel.Error, "Теги не были найдены в базе данных");
                    response.Code = 500;
                    response.Message = "Теги не были найдены в базе данных";
                    return Task.FromResult(response);
                }
                tags.Add(getTagResponse.Result.Item1);
            }
            var dbResponse = PostImporter.CreatePost
                (
                    new Posts()
                    {
                        Title = request.Title,
                        Body = request.Body,
                        CategoryId = getCategoryResponse.Result.Item1.Id,
                        AuthorId = request.AuthorId,
                        CreatedAt = DateTime.UtcNow
                    }
                );
            if (dbResponse.Result.Item2 == null)
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item1);
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
                return Task.FromResult(response);
            }
            else
            {
                foreach(var tagPost in tags)
                {
                    var createTagMessageResponse = TagsPostImporter.CreateTagsPost
                        (
                            new TagsPosts()
                            {
                                PostId = dbResponse.Result.Item2.Id,
                                TagId = tagPost.Id
                            }
                        );
                    if (createTagMessageResponse.Result.Item2 == null)
                    {
                        PostDeleter.DeletePost(dbResponse.Result.Item2.Id);
                        Logger.Logger.Log(LogLevel.Error, createTagMessageResponse.Result.Item1);
                        response.Code = 500;
                        response.Message = createTagMessageResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    tagMessages.Add
                        (
                            new TagMessage()
                            {
                                Id = (uint)tagPost.Id,
                                Name = tagPost.Name
                            }
                        );
                }
                foreach (var contentMeta in contentMetas)
                {
                    var addPostContentResponse = PostContentImporter.CreatePostContent
                        (
                            new PostContent()
                            {
                                PostId = dbResponse.Result.Item2.Id,
                                ContentId = contentMeta.Item1,
                                Marker = contentMeta.Item2,
                                ContentType = contentMeta.Item3
                            }
                        );
                    if (addPostContentResponse.Result.Item2 == null)
                    {
                        PostDeleter.DeletePost(dbResponse.Result.Item2.Id);
                        Logger.Logger.Log(LogLevel.Error, addPostContentResponse.Result.Item1);
                        response.Code = 500;
                        response.Message = addPostContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    var addPostNeo4jResponse = Neo4jPostImporter.AddNewPost(dbResponse.Result.Item2.Id.ToString());
                    if (addPostNeo4jResponse.Result != "OK")
                    {
                        PostDeleter.DeletePost(dbResponse.Result.Item2.Id);
                        Logger.Logger.Log(LogLevel.Error, addPostNeo4jResponse.Result);
                        response.Code = 500;
                        response.Message = addPostNeo4jResponse.Result;
                        return Task.FromResult(response);
                    }
                }
            }
            
            response.PostMessage = new PostMessage()
            {
                Id = dbResponse.Result.Item2.Id.ToString(),
                Title = dbResponse.Result.Item2.Title,
                Body = dbResponse.Result.Item2.Body,
                LikesCount = 0,
                CommentCount = 0,
                ViewsCount = 0,
                BookmarksCount = 0,
                CategoryMessage = new CategoryMessage()
                {
                    Id = (uint)getCategoryResponse.Result.Item1.Id,
                    Title = getCategoryResponse.Result.Item1.Title,
                    ImageFile = getCategoryResponse.Result.Item1.ImageFile.ToString(),
                    Image = getCategoryImageResponse != null ? ByteString.CopyFrom(getCategoryImageResponse.Result.Item2) : ByteString.Empty
                },
                AuthorId = dbResponse.Result.Item2.AuthorId,
                CreatedAt = Timestamp.FromDateTime(dbResponse.Result.Item2.CreatedAt)
            };
            response.PostMessage.PostContentMessage.AddRange(request.PostContentMessage);
            response.PostMessage.Tags.AddRange(tagMessages);
            response.Code = 200;
            response.Message = "OK";
            Logger.Logger.Log(LogLevel.Information, $"Пост {response.PostMessage.Title} добавлен в базу данных");
            return Task.FromResult(response);
        }

        public override Task<AddPostToLikedResponse> AddPostToLiked(AddPostToLikedRequest request, ServerCallContext context)
        {
            var response = new AddPostToLikedResponse();
            if(request.PostId == "" || request.UserId == "")
            {
                Logger.Logger.Log(LogLevel.Error, "Один из параметров был пустой");
                response.Code = 500;
                response.Message = "Один из параметров был пустой";
                return Task.FromResult(response);
            }
            var getPostResponse = PostExporter.GetPost(Guid.Parse(request.PostId));
            if(getPostResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, getPostResponse.Result.Item2);
                response.Code = 500;
                response.Message = getPostResponse.Result.Item2;
                return Task.FromResult(response);
            }
            getPostResponse.Result.Item1.LikesCount++;
            var updatePostResponse = PostUpdater.UpdatePost(getPostResponse.Result.Item1);
            if(updatePostResponse.Result.Item1 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, updatePostResponse.Result.Item1);
                response.Code = 500;
                response.Message = updatePostResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var neo4jResponse = Neo4jRelationshipImporter.AddPostLikeRelationship(request.UserId, request.PostId).Result;
            if(neo4jResponse != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, neo4jResponse);
                response.Code = 500;
                response.Message = neo4jResponse;
                return Task.FromResult(response);
            }
            var neo4jCreateRecommendationsResponse = Neo4jRecomendationRelationshipCreator.CreateNewRecomendations(request.UserId).Result;
            if (neo4jCreateRecommendationsResponse != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, neo4jCreateRecommendationsResponse);
                response.Code = 500;
                response.Message = neo4jCreateRecommendationsResponse;
                return Task.FromResult(response);
            }
            response.Code = 200;
            response.Message = neo4jResponse;
            Logger.Logger
                .Log(
                LogLevel.Information,
                $"Добавдена связь \"Лайк\" между пользователем {request.UserId} и постом {request.PostId}"
                );
            return Task.FromResult(response);
        }

        public override Task<AddPostToViewsResponse> AddPostToViews(AddPostToViewsRequest request, ServerCallContext context)
        {
            var response = new AddPostToViewsResponse();
            if (request.PostId == "" || request.UserId == "")
            {
                Logger.Logger.Log(LogLevel.Error, "Один из параметров был пустой");
                response.Code = 500;
                response.Message = "Один из параметров был пустой";
                return Task.FromResult(response);
            }
            var getPostResponse = PostExporter.GetPost(Guid.Parse(request.PostId));
            if (getPostResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, getPostResponse.Result.Item2);
                response.Code = 500;
                response.Message = getPostResponse.Result.Item2;
                return Task.FromResult(response);
            }
            getPostResponse.Result.Item1.ViewsCount++;
            var updatePostResponse = PostUpdater.UpdatePost(getPostResponse.Result.Item1);
            if (updatePostResponse.Result.Item1 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, updatePostResponse.Result.Item1);
                response.Code = 500;
                response.Message = updatePostResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var neo4jResponse = Neo4jRelationshipImporter.AddPostViewRelationship(request.UserId, request.PostId).Result;
            if (neo4jResponse != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, neo4jResponse);
                response.Code = 500;
                response.Message = neo4jResponse;
                return Task.FromResult(response);
            }
            var neo4jRecommendedRelationshipDeleterResponse =
                Neo4jRecommendedRelationshipDeleter.DeleteRelationship(request.UserId, request.PostId).Result;
            if (neo4jRecommendedRelationshipDeleterResponse != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, neo4jRecommendedRelationshipDeleterResponse);
                response.Code = 500;
                response.Message = neo4jRecommendedRelationshipDeleterResponse;
                return Task.FromResult(response);
            }
            response.Code = 200;
            response.Message = neo4jResponse;
            Logger.Logger.Log(LogLevel.Information,
                $"Добавдена связь \"Просмотрено\" между пользователем {request.UserId} и постом {request.PostId}");
            return Task.FromResult(response);
        }

        public override Task<AddPostToBookmarksResponse> AddPostToBookmarks(AddPostToBookmarksRequest request, ServerCallContext context)
        {
            var response = new AddPostToBookmarksResponse();
            if (request.PostId == "" || request.UserId == "")
            {
                Logger.Logger.Log(LogLevel.Error, "Один из параметров был пустой");
                response.Code = 500;
                response.Message = "Один из параметров был пустой";
                return Task.FromResult(response);
            }
            var getPostResponse = PostExporter.GetPost(Guid.Parse(request.PostId));
            if (getPostResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, getPostResponse.Result.Item2);
                response.Code = 500;
                response.Message = getPostResponse.Result.Item2;
                return Task.FromResult(response);
            }
            getPostResponse.Result.Item1.BookmarksCount++;
            var updatePostResponse = PostUpdater.UpdatePost(getPostResponse.Result.Item1);
            if (updatePostResponse.Result.Item1 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, updatePostResponse.Result.Item1);
                response.Code = 500;
                response.Message = updatePostResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var neo4jResponse = Neo4jRelationshipImporter.AddPostBookmarkRelationship(request.UserId, request.PostId).Result;
            if (neo4jResponse != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, neo4jResponse);
                response.Code = 500;
                response.Message = neo4jResponse;
                return Task.FromResult(response);
            }
            response.Code = 200;
            response.Message = neo4jResponse;
            Logger.Logger.Log(LogLevel.Information,
                $"Добавдена связь \"Избранное\" между пользователем {request.UserId} и постом {request.PostId}");
            return Task.FromResult(response);
        }

        public override Task<DeletePostResponse> DeletePost(DeletePostRequest request, ServerCallContext context)
        {
            DeletePostResponse response = new DeletePostResponse();
            if(request.PostId == "")
            {
                Logger.Logger.Log(LogLevel.Error, "PostId был пустой");
                response.Code = 500;
                response.Message = "PostId был пустой";
                return Task.FromResult(response);
            }
            var getPostResponse = PostExporter.GetPost(Guid.Parse(request.PostId));
            if (getPostResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, getPostResponse.Result.Item2);
                response.Code = 500;
                response.Message = getPostResponse.Result.Item2;
                return Task.FromResult(response);
            }
            var deletePostResponse = PostDeleter.DeletePost(Guid.Parse(request.PostId));
            if(deletePostResponse.Result != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, deletePostResponse.Result);
                response.Code = 500;
                response.Message = deletePostResponse.Result;
                return Task.FromResult(response);
            }
            var neo4jResponse = Neo4jPostDeleter.DeletePost(request.PostId);
            if (neo4jResponse.Result != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, neo4jResponse.Result);
                response.Code = 500;
                response.Message = neo4jResponse.Result;
                return Task.FromResult(response);
            }
            foreach(var content in getPostResponse.Result.Item1.PostContents)
            {
                if(content.ContentType == ContentType.Image)
                {
                    var deletePostContentResponse = Deleter.DeletePostImage(content.ContentId);
                    if(deletePostContentResponse.Result != "OK")
                    {
                        Logger.Logger.Log(LogLevel.Error, deletePostContentResponse.Result);
                        response.Code = 500;
                        response.Message = deletePostContentResponse.Result;
                        return Task.FromResult(response);
                    }
                }
                else
                {
                    var deletePostContentResponse = Deleter.DeletePostVideo(content.ContentId);
                    if (deletePostContentResponse.Result != "OK")
                    {
                        Logger.Logger.Log(LogLevel.Error, deletePostContentResponse.Result);
                        response.Code = 500;
                        response.Message = deletePostContentResponse.Result;
                        return Task.FromResult(response);
                    }
                }
            }
            response.Code = 200;
            response.Message = "OK";
            Logger.Logger.Log(LogLevel.Information,
                $"Пост {request.PostId} удален");
            return Task.FromResult(response);
        }

        public override Task<GetPostsResponse> GetPosts(GetPostsRequest request, ServerCallContext context)
        {
            var response = new GetPostsResponse();
            DateTime dateTimeCursor;
            if(request.Cursor == null)
            {
                dateTimeCursor = DateTime.MaxValue;
            }
            else
            {
                dateTimeCursor = request.Cursor.ToDateTime();
            }
            var cacheKey = $"GetPosts\nUserId: {request.UserId}\nPageSize: {request.PageSize}\nCursor: {dateTimeCursor}";
            var cacheResult = new RedisCacheDbContext().ReadCacheAsync(cacheKey);
            if (cacheResult != null)
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new ByteStringJsonConverter());
                var cache = JsonConvert.DeserializeObject<GetPostsCacheModel>(cacheResult, settings);
                var posts = cache.postMessageList;
                posts = AddContentToPostMessage(posts).Result;
                if (posts.Count != 0)
                {
                    response.Code = 200;
                    response.Message = "OK";
                    response.Entities.AddRange(posts);
                    response.NextCursor = Timestamp.FromDateTime(cache.cursor);
                    Logger.Logger.Log(LogLevel.Information, $"Выданы предзагруженные посты для пользователя {request.UserId}");
                    Task.Run(() => PreloadNextPageGetPostsAsync(request.UserId, (int)request.PageSize, cache.cursor));
                    return Task.FromResult(response);
                }
            }
            var dbResponse = PostExporter.GetPosts(dateTimeCursor, (int)request.PageSize);
            if (dbResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item2);
                response.NextCursor = request.Cursor;
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
                return Task.FromResult(response);
            }
            foreach (var post in dbResponse.Result.Item1)
            {
                var createPostMessageResponse = CreatePostMessage(post);
                if (createPostMessageResponse.Result.Item2 != "OK")
                {
                    Logger.Logger.Log(LogLevel.Error, createPostMessageResponse.Result.Item2);
                    response.NextCursor = request.Cursor;
                    response.Code = 500;
                    response.Message = createPostMessageResponse.Result.Item2;
                    return Task.FromResult(response);
                }
                var postMessage = createPostMessageResponse.Result.Item1;

                var neo4jLikeCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostLikeRelationship(request.UserId, post.Id.ToString()).Result;
                var neo4jViewCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostViewRelationship(request.UserId, post.Id.ToString()).Result;
                var neo4jBookmarkCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostBookmarkRelationship(request.UserId, post.Id.ToString()).Result;

                if
                    (
                    neo4jLikeCheckerResponse.Item1 != "OK" ||
                    neo4jViewCheckerResponse.Item1 != "OK" ||
                    neo4jBookmarkCheckerResponse.Item1 != "OK"
                    )
                {
                    Logger.Logger.Log(LogLevel.Error, "Ошибка работы с Neo4j");
                    response.NextCursor = request.Cursor;
                    response.Code = 500;
                    response.Message = "Ошибка работы с Neo4j";
                    return Task.FromResult(response);
                }

                postMessage.IsLiked = neo4jLikeCheckerResponse.Item2;
                postMessage.IsViewed = neo4jViewCheckerResponse.Item2;
                postMessage.IsBookmarked = neo4jBookmarkCheckerResponse.Item2;

                response.Entities.Add(postMessage);
            }
            response.NextCursor = Timestamp.FromDateTime(dbResponse.Result.Item3);
            response.Code = 200;
            response.Message = "OK";
            Logger.Logger.Log(LogLevel.Information, $"Выполнен запрос на получение постов ({response.Entities.Count})");
            Task.Run(() => PreloadNextPageGetPostsAsync(request.UserId, (int)request.PageSize, dbResponse.Result.Item3));
            return Task.FromResult(response);
        }

        public override Task<GetRecommendedPostsResponse> GetRecommendedPosts(GetRecommendedPostsRequest request, ServerCallContext context)
        {
            var response = new GetRecommendedPostsResponse();
            if (request.UserId == "")
            {
                Logger.Logger.Log(LogLevel.Error, "UserId был пустой");
                response.Code = 500;
                response.Message = "UserId был пустой";
                return Task.FromResult(response);
            }
            var cacheKey = $"GetRecommendedPosts\nUserId: {request.UserId}\nPageSize: {request.PageSize}";
            var cacheResult = new RedisCacheDbContext().ReadCacheAsync(cacheKey);
            if (cacheResult != null)
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new ByteStringJsonConverter());
                var cache = JsonConvert.DeserializeObject<List<PostMessage>>(cacheResult, settings);
                var posts = cache;
                posts = AddContentToPostMessage(posts).Result;
                if (posts.Count != 0)
                {
                    response.Code = 200;
                    response.Message = "OK";
                    response.Entities.AddRange(posts);
                    Logger.Logger.Log(LogLevel.Information, $"Выданы предзагруженные посты для пользователя {request.UserId}");
                    Task.Run(() => PreloadNextPageGetRecommendedPostsAsync(request.UserId, (int)request.PageSize));
                    return Task.FromResult(response);
                }
            }
            var neo4jGetRecommendedPostResponse = Neo4jPostExporter.GetRecommendedPosts(request.UserId, (int)request.PageSize);
            if(neo4jGetRecommendedPostResponse.Result.Item1 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, neo4jGetRecommendedPostResponse.Result.Item1);
                response.Code = 500;
                response.Message = neo4jGetRecommendedPostResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var dbResponse = PostExporter.GetPostsByIdsList(neo4jGetRecommendedPostResponse.Result.Item2);
            if(dbResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item2);
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
                return Task.FromResult(response);
            }
            foreach(var post in dbResponse.Result.Item1)
            {
                var createPostMessageResponse = CreatePostMessage(post);
                if(createPostMessageResponse.Result.Item2 != "OK")
                {
                    Logger.Logger.Log(LogLevel.Error, createPostMessageResponse.Result.Item2);
                    response.Code = 500;
                    response.Message = createPostMessageResponse.Result.Item2;
                    return Task.FromResult(response);
                }
                response.Entities.Add(createPostMessageResponse.Result.Item1);
            }
            response.Code = 200;
            response.Message = "OK";
            Logger.Logger.Log(LogLevel.Information,
                $"Выполнен запрос на получение рекомендованных постов " +
                $"({response.Entities.Count}) для пользователя {request.UserId}");
            Task.Run(() => PreloadNextPageGetRecommendedPostsAsync(request.UserId, (int)request.PageSize));
            return Task.FromResult(response);
        }

        public override Task<GetUserPostsResponse> GetUserPosts(GetUserPostsRequest request, ServerCallContext context)
        {
            var response = new GetUserPostsResponse();
            if (request.UserId == "")
            {
                Logger.Logger.Log(LogLevel.Error, "UserId был пустой");
                response.Code = 500;
                response.Message = "UserId был пустой";
                response.NextCursor = request.Cursor;
                return Task.FromResult(response);
            }
            DateTime dateTimeCursor;
            if (request.Cursor == null)
            {
                dateTimeCursor = DateTime.MaxValue;
            }
            else
            {
                dateTimeCursor = request.Cursor.ToDateTime();
            }
            var dbResponse = PostExporter.GetPostsByUser(dateTimeCursor, (int)request.PageSize, request.UserId);
            if (dbResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item2);
                response.NextCursor = request.Cursor;
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
                return Task.FromResult(response);
            }
            foreach (var post in dbResponse.Result.Item1)
            {
                var createPostMessageResponse = CreatePostMessage(post);
                if (createPostMessageResponse.Result.Item2 != "OK")
                {
                    Logger.Logger.Log(LogLevel.Error, createPostMessageResponse.Result.Item2);
                    response.Code = 500;
                    response.Message = createPostMessageResponse.Result.Item2;
                    response.NextCursor = request.Cursor;
                    return Task.FromResult(response);
                }
                var postMessage = createPostMessageResponse.Result.Item1;

                var neo4jLikeCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostLikeRelationship(request.UserId, post.Id.ToString()).Result;
                var neo4jViewCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostViewRelationship(request.UserId, post.Id.ToString()).Result;
                var neo4jBookmarkCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostBookmarkRelationship(request.UserId, post.Id.ToString()).Result;

                if
                    (
                    neo4jLikeCheckerResponse.Item1 != "OK" ||
                    neo4jViewCheckerResponse.Item1 != "OK" ||
                    neo4jBookmarkCheckerResponse.Item1 != "OK"
                    )
                {
                    Logger.Logger.Log(LogLevel.Error, "Ошибка работы с Neo4j");
                    response.NextCursor = request.Cursor;
                    response.Code = 500;
                    response.Message = "Ошибка работы с Neo4j";
                    return Task.FromResult(response);
                }

                postMessage.IsLiked = neo4jLikeCheckerResponse.Item2;
                postMessage.IsViewed = neo4jViewCheckerResponse.Item2;
                postMessage.IsBookmarked = neo4jBookmarkCheckerResponse.Item2;

                response.Entities.Add(postMessage);
            }
            response.NextCursor = Timestamp.FromDateTime(dbResponse.Result.Item3);
            response.Code = 200;
            response.Message = "OK";
            Logger.Logger
                .Log(LogLevel.Information, $"Выполен запрос на получение постов " +
                $"({response.Entities.Count}) пользователя ({request.UserId})");
            return Task.FromResult(response);

        }

        public override Task<GetBookmarkedPostsResponse> GetBookmarkedPosts(GetBookmarkedPostsRequest request, ServerCallContext context)
        {
            var response = new GetBookmarkedPostsResponse();
            if(request.UserId == "")
            {
                Logger.Logger.Log(LogLevel.Error, "UserId был пустой");
                response.Code = 500;
                response.Message = "UserId был пустой";
                response.NextCursor = request.Cursor;
                return Task.FromResult(response);
            }
            DateTime dateTimeCursor;
            if (request.Cursor == null)
            {
                dateTimeCursor = DateTime.UtcNow;
            }
            else
            {
                dateTimeCursor = request.Cursor.ToDateTime();
            }
            var neo4jGetBookmarkedPostsPostResponse = Neo4jPostExporter.GetBookmarkedPosts(
                request.UserId, 
                (int)request.PageSize+1,
                dateTimeCursor
                );
            if (neo4jGetBookmarkedPostsPostResponse.Result.Item1 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, neo4jGetBookmarkedPostsPostResponse.Result.Item1);
                response.Code = 500;
                response.Message = neo4jGetBookmarkedPostsPostResponse.Result.Item1;
                response.NextCursor = request.Cursor;
                return Task.FromResult(response);
            }
            var dbResponse = PostExporter
                .GetPostsByIdsList(neo4jGetBookmarkedPostsPostResponse.Result.Item2
                    .Take((int)request.PageSize).ToList()
                );
            if (dbResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item2);
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
                response.NextCursor = request.Cursor;
                return Task.FromResult(response);
            }
            foreach (var post in dbResponse.Result.Item1)
            {
                var createPostMessageResponse = CreatePostMessage(post);
                if (createPostMessageResponse.Result.Item2 != "OK")
                {
                    Logger.Logger.Log(LogLevel.Error, createPostMessageResponse.Result.Item2);
                    response.Code = 500;
                    response.Message = createPostMessageResponse.Result.Item2;
                    response.NextCursor = request.Cursor;
                    return Task.FromResult(response);
                }
                response.Entities.Add(createPostMessageResponse.Result.Item1);
            }
            response.Code = 200;
            response.Message = "OK";
            response.NextCursor = Timestamp
                .FromDateTime(neo4jGetBookmarkedPostsPostResponse.Result.Item3);
            Logger.Logger.Log(LogLevel.Information, $"Выполнен запрос на получение постов ({response.Entities.Count}), " +
                $"добавленных пользователем ({request.UserId}) в избранное");
            return Task.FromResult(response);
        }

        public override Task<UpdateBodyPostResoponse> UpdateBodyPost(UpdateBodyPostRequest request, ServerCallContext context)
        {
            var response = new UpdateBodyPostResoponse();
            if(request.PostId == "")
            {
                Logger.Logger.Log(LogLevel.Error, "PostId был пустой");
                response.Code = 500;
                response.Message = "PostId был пустой";
                return Task.FromResult(response);
            }
            var getPostResponse = PostExporter.GetPost(Guid.Parse(request.PostId));
            if (getPostResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, getPostResponse.Result.Item2);
                response.Code = 500;
                response.Message = getPostResponse.Result.Item2;
                return Task.FromResult(response);
            }
            var post = getPostResponse.Result.Item1;
            post.Body = request.Body;
            post.UpdatedAt = DateTime.UtcNow; 
            var dbResponse = PostUpdater.UpdatePost(post);
            if (dbResponse.IsCanceled)
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item1);
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
                return Task.FromResult(response);
            }
            else if (dbResponse.Result.Item2 == null)
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item1);
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var createPostMessageResponse = CreatePostMessage(post);
            if (createPostMessageResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, createPostMessageResponse.Result.Item2);
                response.Code = 500;
                response.Message = createPostMessageResponse.Result.Item2;
                return Task.FromResult(response);
            }
            response.Post = createPostMessageResponse.Result.Item1;
            response.Code = 200;
            response.Message = "OK";
            Logger.Logger.Log(LogLevel.Information, $"Пост {response.Post.Title} был обновлен");
            return Task.FromResult(response);
        }

        public override Task<GetPostsByCategoryResponse> GetPostsByCategory(GetPostsByCategoryRequest request, ServerCallContext context)
        {
            var response = new GetPostsByCategoryResponse();
            if(request.CategoryId == 0)
            {
                Logger.Logger.Log(LogLevel.Error, "CategoryId было 0");
                response.NextCursor = request.Cursor;
                response.Code = 500;
                response.Message = "CategoryId было 0";
                return Task.FromResult(response);
            }
            DateTime dateTimeCursor;
            if (request.Cursor == null)
            {
                dateTimeCursor = DateTime.MaxValue;
            }
            else
            {
                dateTimeCursor = request.Cursor.ToDateTime();
            }
            var dbResponse = PostExporter.GetPostsByCategoryId(dateTimeCursor, (int)request.PageSize, (int)request.CategoryId);
            if (dbResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item2);
                response.NextCursor = request.Cursor;
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
                return Task.FromResult(response);
            }
            foreach (var post in dbResponse.Result.Item1)
            {
                var createPostMessageResponse = CreatePostMessage(post);
                if (createPostMessageResponse.Result.Item2 != "OK")
                {
                    Logger.Logger.Log(LogLevel.Error, createPostMessageResponse.Result.Item2);
                    response.Code = 500;
                    response.Message = createPostMessageResponse.Result.Item2;
                    response.NextCursor = request.Cursor;
                    return Task.FromResult(response);
                }
                var postMessage = createPostMessageResponse.Result.Item1;

                var neo4jLikeCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostLikeRelationship(request.UserId, post.Id.ToString()).Result;
                var neo4jViewCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostViewRelationship(request.UserId, post.Id.ToString()).Result;
                var neo4jBookmarkCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostBookmarkRelationship(request.UserId, post.Id.ToString()).Result;

                if
                    (
                    neo4jLikeCheckerResponse.Item1 != "OK" ||
                    neo4jViewCheckerResponse.Item1 != "OK" ||
                    neo4jBookmarkCheckerResponse.Item1 != "OK"
                    )
                {
                    Logger.Logger.Log(LogLevel.Error, "Ошибка работы с Neo4j");
                    response.NextCursor = request.Cursor;
                    response.Code = 500;
                    response.Message = "Ошибка работы с Neo4j";
                    return Task.FromResult(response);
                }

                postMessage.IsLiked = neo4jLikeCheckerResponse.Item2;
                postMessage.IsViewed = neo4jViewCheckerResponse.Item2;
                postMessage.IsBookmarked = neo4jBookmarkCheckerResponse.Item2;

                response.Entities.Add(postMessage);
            }
            response.NextCursor = Timestamp.FromDateTime(dbResponse.Result.Item3);
            response.Code = 200;
            response.Message = "OK";
            Logger.Logger
                .Log(LogLevel.Information, $"Выполнен запрос на получение постов ({response.Entities.Count})," +
                $" категория которых {request.CategoryId}");
            return Task.FromResult(response);
        }

        public override Task<GetPostsByTagsResponse> GetPostsByTags(GetPostsByTagsRequest request, ServerCallContext context)
        {
            var response = new GetPostsByTagsResponse();
            if(request.TagIds.ToList().Count == 0)
            {
                Logger.Logger.Log(LogLevel.Error, "TagIds было пустое");
                response.NextCursor = request.Cursor;
                response.Code = 500;
                response.Message = "TagIds было пустое";
                return Task.FromResult(response);
            }
            DateTime dateTimeCursor;
            if (request.Cursor == null)
            {
                dateTimeCursor = DateTime.MaxValue;
            }
            else
            {
                dateTimeCursor = request.Cursor.ToDateTime();
            }
            var dbResponse = PostExporter
                .GetPostsByTagsId(
                    dateTimeCursor,
                    (int)request.PageSize,
                    request.TagIds.ToList().ConvertAll(o=> (int)o)
                );
            if (dbResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item2);
                response.NextCursor = request.Cursor;
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
                return Task.FromResult(response);
            }
            foreach (var post in dbResponse.Result.Item1)
            {
                var createPostMessageResponse = CreatePostMessage(post);
                if (createPostMessageResponse.Result.Item2 != "OK")
                {
                    Logger.Logger.Log(LogLevel.Error, createPostMessageResponse.Result.Item2);
                    response.Code = 500;
                    response.Message = createPostMessageResponse.Result.Item2;
                    response.NextCursor = request.Cursor;
                    return Task.FromResult(response);
                }
                var postMessage = createPostMessageResponse.Result.Item1;

                var neo4jLikeCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostLikeRelationship(request.UserId, post.Id.ToString()).Result;
                var neo4jViewCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostViewRelationship(request.UserId, post.Id.ToString()).Result;
                var neo4jBookmarkCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostBookmarkRelationship(request.UserId, post.Id.ToString()).Result;

                if
                    (
                    neo4jLikeCheckerResponse.Item1 != "OK" ||
                    neo4jViewCheckerResponse.Item1 != "OK" ||
                    neo4jBookmarkCheckerResponse.Item1 != "OK"
                    )
                {
                    Logger.Logger.Log(LogLevel.Error, "Ошибка работы с Neo4j");
                    response.NextCursor = request.Cursor;
                    response.Code = 500;
                    response.Message = "Ошибка работы с Neo4j";
                    return Task.FromResult(response);
                }

                postMessage.IsLiked = neo4jLikeCheckerResponse.Item2;
                postMessage.IsViewed = neo4jViewCheckerResponse.Item2;
                postMessage.IsBookmarked = neo4jBookmarkCheckerResponse.Item2;

                response.Entities.Add(postMessage);
            }
            response.NextCursor = Timestamp.FromDateTime(dbResponse.Result.Item3);
            response.Code = 200;
            response.Message = "OK";
            Logger.Logger.Log(LogLevel.Information, $"Получены посты ({response.Entities.Count})," +
                $" теги которых ({string.Join(", ", request.TagIds.ToList())})");
            return Task.FromResult(response);
        }

        public async static Task<(PostMessage, string)> CreatePostMessage(Posts post)
        {
            (PostMessage, string) response = (null, "");
            var postMessage = new PostMessage()
            {
                Id = post.Id.ToString(),
                Title = post.Title,
                Body = post.Body,
                CategoryMessage = new CategoryMessage()
                {
                    Id = (uint)post.CategoryPost.Id,
                    Title = post.CategoryPost.Title,
                    ImageFile = post.CategoryPost.ImageFile.ToString(),
                    Image = ByteString.CopyFrom(Exporter.GetCategoryImage(post.CategoryPost.ImageFile).Result.Item2)
                },
                LikesCount = (uint)post.LikesCount,
                ViewsCount = (uint)post.ViewsCount,
                BookmarksCount = (uint)post.BookmarksCount,
                CommentCount = (uint)post.CommentCount,
                AuthorId = post.AuthorId,
                CreatedAt = Timestamp.FromDateTime(post.CreatedAt),
                UpdatedAt = Timestamp.FromDateTimeOffset(post.UpdatedAt)
            };

            var getTagsPostResponse = TagsPostExporter.GetTagsPost(post.Id);
            if (getTagsPostResponse.Result.Item2 != "OK")
            {
                response.Item2 = getTagsPostResponse.Result.Item2;
                return response;
            }
            foreach (var tagPost in getTagsPostResponse.Result.Item1)
            {
                postMessage.Tags.Add
                    (
                        new TagMessage()
                        {
                            Id = (uint)tagPost.Tag.Id,
                            Name = tagPost.Tag.Name,
                        }
                    );
            }

            var getPostContentResponse = PostContentExporter.GetContentPost(post.Id);
            if (getPostContentResponse.Result.Item2 != "OK")
            {
                response.Item2 = getPostContentResponse.Result.Item2;
                return response;
            }
            foreach (var content in getPostContentResponse.Result.Item1)
            {
                if (content.ContentType.Equals(ContentType.Image))
                {
                    var postContentResponse = Exporter.GetPostImage(content.ContentId);
                    if (postContentResponse.Result.Item1 != "OK")
                    {
                        response.Item2 = postContentResponse.Result.Item1;
                        return response;
                    }
                    postMessage.PostContentMessage.Add
                        (
                            new PostContentMessage()
                            {
                                ContentId = content.ContentId.ToString(),
                                Content = ByteString.CopyFrom(postContentResponse.Result.Item2),
                                Marker = content.Marker,
                                ContentTypeEnum = ContentTypeEnum.Image
                            }
                        );
                }
                else
                {
                    var postContentResponse = Exporter.GetPostVideos(content.ContentId);
                    if (postContentResponse.Result.Item1 != "OK")
                    {
                        response.Item2 = postContentResponse.Result.Item1;
                        return response;
                    }
                    postMessage.PostContentMessage.Add
                        (
                            new PostContentMessage()
                            {
                                ContentId = content.ContentId.ToString(),
                                Content = ByteString.CopyFrom(postContentResponse.Result.Item2),
                                Marker = content.Marker,
                                ContentTypeEnum = ContentTypeEnum.Video
                            }
                        );
                }
            }
            response.Item1 = postMessage;
            response.Item2 = "OK";
            return response;
        }

        public async static Task<(PostMessage, string)> CreatePostMessageWithoutContent(Posts post)
        {
            (PostMessage, string) response = (null, "");
            var postMessage = new PostMessage()
            {
                Id = post.Id.ToString(),
                Title = post.Title,
                Body = post.Body,
                CategoryMessage = new CategoryMessage()
                {
                    Id = (uint)post.CategoryPost.Id,
                    Title = post.CategoryPost.Title,
                    ImageFile = post.CategoryPost.ImageFile.ToString(),
                    Image = ByteString.CopyFrom(Exporter.GetCategoryImage(post.CategoryPost.ImageFile).Result.Item2)
                },
                LikesCount = (uint)post.LikesCount,
                ViewsCount = (uint)post.ViewsCount,
                BookmarksCount = (uint)post.BookmarksCount,
                CommentCount = (uint)post.CommentCount,
                AuthorId = post.AuthorId,
                CreatedAt = Timestamp.FromDateTime(post.CreatedAt),
                UpdatedAt = Timestamp.FromDateTimeOffset(post.UpdatedAt)
            };

            var getTagsPostResponse = TagsPostExporter.GetTagsPost(post.Id);
            if (getTagsPostResponse.Result.Item2 != "OK")
            {
                response.Item2 = getTagsPostResponse.Result.Item2;
                return response;
            }
            foreach (var tagPost in getTagsPostResponse.Result.Item1)
            {
                postMessage.Tags.Add
                    (
                        new TagMessage()
                        {
                            Id = (uint)tagPost.Tag.Id,
                            Name = tagPost.Tag.Name,
                        }
                    );
            }

            var getPostContentResponse = PostContentExporter.GetContentPost(post.Id);
            if (getPostContentResponse.Result.Item2 != "OK")
            {
                response.Item2 = getPostContentResponse.Result.Item2;
                return response;
            }
            foreach (var content in getPostContentResponse.Result.Item1)
            {
                if (content.ContentType.Equals(ContentType.Image))
                {
                    
                    postMessage.PostContentMessage.Add
                        (
                            new PostContentMessage()
                            {
                                ContentId = content.ContentId.ToString(),
                                Marker = content.Marker,
                                ContentTypeEnum = ContentTypeEnum.Image
                            }
                        );
                }
                else
                {
                    postMessage.PostContentMessage.Add
                        (
                            new PostContentMessage()
                            {
                                ContentId = content.ContentId.ToString(),
                                Marker = content.Marker,
                                ContentTypeEnum = ContentTypeEnum.Video
                            }
                        );
                }
            }
            response.Item1 = postMessage;
            response.Item2 = "OK";
            return response;
        }

        public async Task<List<PostMessage>> AddContentToPostMessage(List<PostMessage> postMessages)
        {
            foreach (var post in postMessages)
            {
                foreach (var content in post.PostContentMessage)
                {
                    if (content.ContentTypeEnum.Equals(ContentTypeEnum.Image))
                    {
                        var postContentResponse = Exporter.GetPostImage(Guid.Parse(content.ContentId));
                        if (postContentResponse.Result.Item1 != "OK")
                        {
                            Logger.Logger.Log(LogLevel.Error, postContentResponse.Result.Item1);
                            return null;
                        }
                        content.Content = ByteString.CopyFrom(postContentResponse.Result.Item2);
                    }
                    else
                    {
                        var postContentResponse = Exporter.GetPostVideos(Guid.Parse(content.ContentId));
                        if (postContentResponse.Result.Item1 != "OK")
                        {
                            Logger.Logger.Log(LogLevel.Error, postContentResponse.Result.Item1);
                            return null;
                        }
                        ByteString.CopyFrom(postContentResponse.Result.Item2);
                    }
                }
            }
            return postMessages;
        }

        private async Task PreloadNextPageGetPostsAsync(string userId, int pageSize, DateTime cursor)
        {
            List<PostMessage> postMessages = new List<PostMessage>();
            var dbResponse = PostExporter.GetPosts(cursor, pageSize);
            if (dbResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item2);
                return;
            }
            foreach (var post in dbResponse.Result.Item1)
            {
                var createPostMessageResponse = CreatePostMessageWithoutContent(post);
                if (createPostMessageResponse.Result.Item2 != "OK")
                {
                    Logger.Logger.Log(LogLevel.Error, createPostMessageResponse.Result.Item2);
                    return;
                }
                var postMessage = createPostMessageResponse.Result.Item1;

                var neo4jLikeCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostLikeRelationship(userId, post.Id.ToString()).Result;
                var neo4jViewCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostViewRelationship(userId, post.Id.ToString()).Result;
                var neo4jBookmarkCheckerResponse = Neo4jRelationshipExistChecker.CheckExistPostBookmarkRelationship(userId, post.Id.ToString()).Result;

                if
                    (
                    neo4jLikeCheckerResponse.Item1 != "OK" ||
                    neo4jViewCheckerResponse.Item1 != "OK" ||
                    neo4jBookmarkCheckerResponse.Item1 != "OK"
                    )
                {
                    Logger.Logger.Log(LogLevel.Error, "Ошибка работы с Neo4j");
                    return;
                }

                postMessage.IsLiked = neo4jLikeCheckerResponse.Item2;
                postMessage.IsViewed = neo4jViewCheckerResponse.Item2;
                postMessage.IsBookmarked = neo4jBookmarkCheckerResponse.Item2;

                postMessages.Add(postMessage);
            }
            DateTime nextCursor = dbResponse.Result.Item3;
            GetPostsCacheModel getPostsCacheModel = new GetPostsCacheModel();
            getPostsCacheModel.cursor = nextCursor;
            getPostsCacheModel.postMessageList.AddRange(postMessages);
            var cache = getPostsCacheModel.Serialize();
            var cacheKey = $"GetPosts\nUserId: {userId}\nPageSize: {pageSize}\nCursor: {cursor}";
            RedisCacheDbContext db = new RedisCacheDbContext();
            db.WriteAsync(cacheKey, cache);
            Logger.Logger.Log(LogLevel.Information, $"Выполнен запрос на предзагрузку постов ({postMessages.Count})");
        }

        private async Task PreloadNextPageGetRecommendedPostsAsync(string userId, int pageSize)
        {
            List<PostMessage> postMessages = new List<PostMessage>();
            var neo4jGetRecommendedPostResponse = Neo4jPostExporter.GetRecommendedPosts(userId, (int)pageSize);
            if (neo4jGetRecommendedPostResponse.Result.Item1 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, neo4jGetRecommendedPostResponse.Result.Item1);
                return;
            }
            var dbResponse = PostExporter.GetPostsByIdsList(neo4jGetRecommendedPostResponse.Result.Item2);
            if (dbResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item2);
                return;
            }
            foreach (var post in dbResponse.Result.Item1)
            {
                var createPostMessageResponse = CreatePostMessageWithoutContent(post);
                if (createPostMessageResponse.Result.Item2 != "OK")
                {
                    Logger.Logger.Log(LogLevel.Error, createPostMessageResponse.Result.Item2);
                    return;
                }
                postMessages.Add(createPostMessageResponse.Result.Item1);
            }
            var cache = JsonConvert.SerializeObject(postMessages);
            var cacheKey = $"GetRecommendedPosts\nUserId: {userId}\nPageSize: {pageSize}";
            RedisCacheDbContext db = new RedisCacheDbContext();
            db.WriteAsync(cacheKey, cache);
            Logger.Logger.Log(LogLevel.Information, $"Выполнен запрос на предзагрузку постов ({postMessages.Count})");
        }
    }

}
