using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Raven.DB.MinIO;
using Raven.DB.Neo4j.Creator;
using Raven.DB.Neo4j.Exporters;
using Raven.DB.Neo4j.Importers;
using Raven.DB.PSQL.Entity;
using Raven.DB.PSQL.Entity.@enum;
using Raven.DB.PSQL.gRPC.Deleters;
using Raven.DB.PSQL.gRPC.Exporters;
using Raven.DB.PSQL.gRPC.Importers;
using Raven.DB.PSQL.gRPC.Updaters;

namespace Raven.Services
{
    public class CommentService : CommentHandler.CommentHandlerBase
    {
        public override Task<CreateCommentToPostResponse> CreateCommentToPost(CreateCommentToPostRequest request, ServerCallContext context)
        {
            var response = new CreateCommentToPostResponse();
            List<(Guid, string, ContentType, byte[])> contentMetas = new List<(Guid, string, ContentType, byte[])>();
            foreach (var content in request.CommentContentMessage)
            {
                if (content.ContentTypeEnum.ToString().Equals("Image"))
                {
                    var addContentResponse = Importer.AddNewCommentImage(content.Content.ToByteArray());
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
                    var addContentResponse = Importer.AddNewCommentVideo(content.Content.ToByteArray());
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
            var dbResponse = CommentImporter.CreateComment
                (
                    new Comments()
                    {
                        Body = request.Body,
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
            foreach (var contentMeta in contentMetas)
            {
                var addCommentContentResponse = CommentContentImporter.CreateCommentContent
                    (
                        new CommentContent()
                        {
                            CommentId = dbResponse.Result.Item2.Id,
                            ContentId = contentMeta.Item1,
                            Marker = contentMeta.Item2,
                            ContentType = contentMeta.Item3
                        }
                    );
                if (addCommentContentResponse.Result.Item2 == null)
                {
                    Logger.Logger.Log(LogLevel.Error, addCommentContentResponse.Result.Item1);
                    CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                    response.Code = 500;
                    response.Message = addCommentContentResponse.Result.Item1;
                    return Task.FromResult(response);
                }
            }
            var getPostResponse = PostExporter.GetPost(Guid.Parse(request.PostId));
            if (getPostResponse.Result.Item2 != "OK" || getPostResponse.Result.Item1 == null)
            {
                Logger.Logger.Log(LogLevel.Error, getPostResponse.Result.Item2);
                CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                response.Code = 500;
                response.Message = getPostResponse.Result.Item2;
                return Task.FromResult(response);
            }
            getPostResponse.Result.Item1.CommentCount++;
            var updatePostResponse = PostUpdater.UpdatePost(getPostResponse.Result.Item1);
            if (updatePostResponse.Result.Item1 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, updatePostResponse.Result.Item1);
                CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                response.Code = 500;
                response.Message = updatePostResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var addPostNeo4jResponse = Neo4jCommentImporter.AddNewCommentToPost
                (
                    request.PostId,
                    dbResponse.Result.Item2.Id.ToString()
                );
            if (addPostNeo4jResponse.Result != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, addPostNeo4jResponse.Result);
                CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                response.Code = 500;
                response.Message = addPostNeo4jResponse.Result;
                return Task.FromResult(response);
            }
            response.CommentMessage = new CommentMessage()
            {
                Id = dbResponse.Result.Item2.Id.ToString(),
                Body = dbResponse.Result.Item2.Body,
                LikesCount = 0,
                CommentCount = 0,
                AuthorId = dbResponse.Result.Item2.AuthorId,
                CreatedAt = Timestamp.FromDateTime(dbResponse.Result.Item2.CreatedAt)
            };
            response.CommentMessage.CommentContentMessage.AddRange(request.CommentContentMessage);
            response.Code = 200;
            response.Message = "OK";
            Logger.Logger
                .Log(LogLevel.Information, $"Пользователь {request.AuthorId} прокомментировал пост ({request.PostId})");
            return Task.FromResult(response);
        }

        public override Task<CreateCommentToCommentResponse> CreateCommentToComment(CreateCommentToCommentRequest request, ServerCallContext context)
        {
            var response = new CreateCommentToCommentResponse();
            List<(Guid, string, ContentType, byte[])> contentMetas = new List<(Guid, string, ContentType, byte[])>();
            foreach (var content in request.CommentContentMessage)
            {
                if (content.ContentTypeEnum.ToString().Equals("Image"))
                {
                    var addContentResponse = Importer.AddNewCommentImage(content.Content.ToByteArray());
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
                    var addContentResponse = Importer.AddNewCommentVideo(content.Content.ToByteArray());
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
            var dbResponse = CommentImporter.CreateComment
                (
                    new Comments()
                    {
                        Body = request.Body,
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
            foreach (var contentMeta in contentMetas)
            {
                var addCommentContentResponse = CommentContentImporter.CreateCommentContent
                    (
                        new CommentContent()
                        {
                            CommentId = dbResponse.Result.Item2.Id,
                            ContentId = contentMeta.Item1,
                            Marker = contentMeta.Item2,
                            ContentType = contentMeta.Item3
                        }
                    );
                if (addCommentContentResponse.Result.Item2 == null)
                {
                    Logger.Logger.Log(LogLevel.Error, addCommentContentResponse.Result.Item1);
                    CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                    response.Code = 500;
                    response.Message = addCommentContentResponse.Result.Item1;
                    return Task.FromResult(response);
                }
            }
            var getCommentResponse = CommentExporter.GetComment(Guid.Parse(request.CommentId));
            if (getCommentResponse.Result.Item2 != "OK" || getCommentResponse.Result.Item1 == null)
            {
                Logger.Logger.Log(LogLevel.Error, getCommentResponse.Result.Item2);
                CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                response.Code = 500;
                response.Message = getCommentResponse.Result.Item2;
                return Task.FromResult(response);
            }
            getCommentResponse.Result.Item1.CommentCount++;
            var updateCommentResponse = CommentUpdater.UpdateComment(getCommentResponse.Result.Item1);
            if (updateCommentResponse.Result.Item1 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, updateCommentResponse.Result.Item1);
                CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                response.Code = 500;
                response.Message = updateCommentResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var getPostResponse = PostExporter.GetPost(Guid.Parse(request.PostId));
            if (getPostResponse.Result.Item2 != "OK" || getPostResponse.Result.Item1 == null)
            {
                Logger.Logger.Log(LogLevel.Error, getPostResponse.Result.Item2);
                CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                response.Code = 500;
                response.Message = getPostResponse.Result.Item2;
                return Task.FromResult(response);
            }
            getPostResponse.Result.Item1.CommentCount++;
            var updatePostResponse = PostUpdater.UpdatePost(getPostResponse.Result.Item1);
            if (updatePostResponse.Result.Item1 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, updatePostResponse.Result.Item1);
                CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                response.Code = 500;
                response.Message = updatePostResponse.Result.Item1;
                return Task.FromResult(response);
            }

            var addPostNeo4jResponse = Neo4jCommentImporter.AddNewCommentToComment
                (
                    request.CommentId,
                    dbResponse.Result.Item2.Id.ToString()
                );
            if (addPostNeo4jResponse.Result != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, addPostNeo4jResponse.Result);
                CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                response.Code = 500;
                response.Message = addPostNeo4jResponse.Result;
                return Task.FromResult(response);
            }
            response.CommentMessage = new CommentMessage()
            {
                Id = dbResponse.Result.Item2.Id.ToString(),
                Body = dbResponse.Result.Item2.Body,
                LikesCount = 0,
                CommentCount = 0,
                AuthorId = dbResponse.Result.Item2.AuthorId,
                CreatedAt = Timestamp.FromDateTime(dbResponse.Result.Item2.CreatedAt)
            };
            response.CommentMessage.CommentContentMessage.AddRange(request.CommentContentMessage);
            response.Code = 200;
            response.Message = "OK";
            Logger.Logger
                .Log(LogLevel.Information, $"Пользователь {request.AuthorId} прокомментировал комментарий ({request.CommentId})");
            return Task.FromResult(response);
        }

        public override Task<GetCommentsToPostResponse> GetCommentsToPost(GetCommentsToPostRequest request, ServerCallContext context)
        {
            var response = new GetCommentsToPostResponse();
            if(request.PostId == "")
            {
                Logger.Logger.Log(LogLevel.Error, "PostId был пустой");
                response.Code = 500;
                response.Message = "PostId был пустой";
                return Task.FromResult(response);
            }
            var neo4jGetCommentsToPostResponse = Neo4jCommentExporter.GetCommentsToPost(request.PostId);
            if (neo4jGetCommentsToPostResponse.Result.Item1 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, neo4jGetCommentsToPostResponse.Result.Item1);
                response.Code = 500;
                response.Message = neo4jGetCommentsToPostResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var dbResponse = CommentExporter.GetCommentsByIdsList(neo4jGetCommentsToPostResponse.Result.Item2);
            if (dbResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item2);
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
                return Task.FromResult(response);
            }
            foreach (var post in dbResponse.Result.Item1)
            {
                var createPostMessageResponse = CreateCommentMessage(post);
                if (createPostMessageResponse.Result.Item2 != "OK")
                {
                    Logger.Logger.Log(LogLevel.Error, createPostMessageResponse.Result.Item2);
                    response.Code = 500;
                    response.Message = createPostMessageResponse.Result.Item2;
                    return Task.FromResult(response);
                }
                response.Entities.Add(createPostMessageResponse.Result.Item1);
            }
            response.Message = "OK";
            response.Code = 200;
            Logger.Logger.Log(LogLevel.Information, $"Выполнен запрос на получение комментариев " +
                $"({response.Entities.Count}) к посту ({request.PostId})");
            return Task.FromResult(response);
        }

        public override Task<GetCommentsToCommentResponse> GetCommentsToComment(GetCommentsToCommentRequest request, ServerCallContext context)
        {
            var response = new GetCommentsToCommentResponse();
            if (request.CommentId == "")
            {
                Logger.Logger.Log(LogLevel.Error, "CommentId был пустой");
                response.Code = 500;
                response.Message = "CommentId был пустой";
                return Task.FromResult(response);
            }
            var neo4jGetCommentsToPostResponse = Neo4jCommentExporter.GetCommentsToComment(request.CommentId);
            if (neo4jGetCommentsToPostResponse.Result.Item1 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, neo4jGetCommentsToPostResponse.Result.Item1);
                response.Code = 500;
                response.Message = neo4jGetCommentsToPostResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var dbResponse = CommentExporter.GetCommentsByIdsList(neo4jGetCommentsToPostResponse.Result.Item2);
            if (dbResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item2);
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
                return Task.FromResult(response);
            }
            foreach (var post in dbResponse.Result.Item1)
            {
                var createPostMessageResponse = CreateCommentMessage(post);
                if (createPostMessageResponse.Result.Item2 != "OK")
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
            Logger.Logger
                .Log(LogLevel.Information, $"Выполнен запрос на получение комментариев ({response.Entities.Count}) " +
                $"к комментарию ({request.CommentId})");
            return Task.FromResult(response);
        }

        public override Task<UpdateCommentResponse> UpdateComment(UpdateCommentRequest request, ServerCallContext context)
        {
            var response = new UpdateCommentResponse();
            if (request.CommentId == "")
            {
                Logger.Logger.Log(LogLevel.Error, "CommentId был пустой");
                response.Code = 500;
                response.Message = "CommentId был пустой";
                return Task.FromResult(response);
            }
            var getCommentResponse = CommentExporter.GetComment(Guid.Parse(request.CommentId));
            if (getCommentResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log (LogLevel.Error, getCommentResponse.Result.Item2);
                response.Code = 500;
                response.Message = getCommentResponse.Result.Item2;
                return Task.FromResult(response);
            }
            var comment = getCommentResponse.Result.Item1;
            comment.Body = request.Body;
            comment.UpdatedAt = DateTime.UtcNow;
            var dbResponse = CommentUpdater.UpdateComment(comment);
            if (dbResponse.Result.Item2 == null)
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item1);
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var createPostMessageResponse = CreateCommentMessage(comment);
            if (createPostMessageResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, createPostMessageResponse.Result.Item2);
                response.Code = 500;
                response.Message = createPostMessageResponse.Result.Item2;
                return Task.FromResult(response);
            }
            response.Comment = createPostMessageResponse.Result.Item1;
            response.Code = 200;
            response.Message = "OK";
            Logger.Logger.Log(LogLevel.Information, $"Выполнен запрос на обновление комментария {request.CommentId}");
            return Task.FromResult(response);
        }

        public override Task<DeleteCommentResponse> DeleteComment(DeleteCommentRequest request, ServerCallContext context)
        {
            var response = new DeleteCommentResponse();
            if (request.CommentId == "")
            {
                Logger.Logger.Log(LogLevel.Error, "CommentId был пустой");
                response.Code = 500;
                response.Message = "CommentId был пустой";
                return Task.FromResult(response);
            }
            var getCommentResponse = CommentExporter.GetComment(Guid.Parse(request.CommentId));
            if (getCommentResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, getCommentResponse.Result.Item2);
                response.Code = 500;
                response.Message = getCommentResponse.Result.Item2;
                return Task.FromResult(response);
            }
            var comment = getCommentResponse.Result.Item1;
            comment.IsRemoved = true;
            var dbResponse = CommentUpdater.UpdateComment(comment);
            if (dbResponse.Result.Item2 == null)
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item1);
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var createPostMessageResponse = CreateCommentMessage(comment);
            if (createPostMessageResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, createPostMessageResponse.Result.Item2);
                response.Code = 500;
                response.Message = createPostMessageResponse.Result.Item2;
                return Task.FromResult(response);
            }
            response.Code = 200;
            response.Message = "OK";
            Logger.Logger.Log(LogLevel.Information, $"Комментарий ({request.CommentId}) был отмечен удаленным");
            return Task.FromResult(response);
        }

        public override Task<LikeCommentResponse> LikeComment(LikeCommentRequest request, ServerCallContext context)
        {
            var response = new LikeCommentResponse();
            if (request.CommentId == "")
            {
                Logger.Logger.Log(LogLevel.Error, "CommentId был пустой");
                response.Code = 500;
                response.Message = "CommentId был пустой";
                return Task.FromResult(response);
            }
            if (request.UserId == "")
            {
                Logger.Logger.Log(LogLevel.Error, "UserId был пустой");
                response.Code = 500;
                response.Message = "UserId был пустой";
                return Task.FromResult(response);
            }
            var getCommentResponse = CommentExporter.GetComment(Guid.Parse(request.CommentId));
            if (getCommentResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, getCommentResponse.Result.Item2);
                response.Code = 500;
                response.Message = getCommentResponse.Result.Item2;
                return Task.FromResult(response);
            }
            getCommentResponse.Result.Item1.LikesCount++;
            var updatePostResponse = CommentUpdater.UpdateComment(getCommentResponse.Result.Item1);
            if (updatePostResponse.Result.Item1 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, updatePostResponse.Result.Item1);
                response.Code = 500;
                response.Message = updatePostResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var neo4jResponse = Neo4jRelationshipImporter.AddCommentLikeRelationship(request.UserId, request.CommentId).Result;
            if (neo4jResponse != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, neo4jResponse);
                response.Code = 500;
                response.Message = neo4jResponse;
                return Task.FromResult(response);
            }
            response.Code = 200;
            response.Message = neo4jResponse;
            Logger.Logger
                .Log(LogLevel.Error, $"Добавдена связь \"Лайк\" между пользователем {request.UserId}" +
                $" и комментарием {request.CommentId}");
            return Task.FromResult(response);
        }

        public async static Task<(CommentMessage, string)> CreateCommentMessage(Comments comment)
        {
            (CommentMessage, string) response = (null, "");
            var commentMessage = new CommentMessage()
            {
                Id = comment.Id.ToString(),
                Body = comment.Body,
                LikesCount = (uint)comment.LikesCount,
                CommentCount = (uint)comment.CommentCount,
                AuthorId = comment.AuthorId,
                CreatedAt = Timestamp.FromDateTime(comment.CreatedAt),
                UpdatedAt = Timestamp.FromDateTimeOffset(comment.UpdatedAt)
            };


            var getCommentContentResponse = CommentContentExporter.GetContentComment(comment.Id);
            if (getCommentContentResponse.Result.Item2 != "OK")
            {
                response.Item2 = getCommentContentResponse.Result.Item2;
                return response;
            }
            foreach (var content in getCommentContentResponse.Result.Item1)
            {
                if (content.ContentType.Equals(ContentType.Image))
                {
                    var postContentResponse = Exporter.GetCommentImage(content.ContentId);
                    if (postContentResponse.Result.Item1 != "OK")
                    {
                        response.Item2 = postContentResponse.Result.Item1;
                        return response;
                    }
                    commentMessage.CommentContentMessage.Add
                        (
                            new CommentContentMessage()
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
                    var postContentResponse = Exporter.GetCommentVideos(content.ContentId);
                    if (postContentResponse.Result.Item1 != "OK")
                    {
                        response.Item2 = postContentResponse.Result.Item1;
                        return response;
                    }
                    commentMessage.CommentContentMessage.Add
                        (
                            new CommentContentMessage()
                            {
                                ContentId = content.ContentId.ToString(),
                                Content = ByteString.CopyFrom(postContentResponse.Result.Item2),
                                Marker = content.Marker,
                                ContentTypeEnum = ContentTypeEnum.Video
                            }
                        );
                }
            }
            response.Item1 = commentMessage;
            response.Item2 = "OK";
            return response;
        }
    }
}
