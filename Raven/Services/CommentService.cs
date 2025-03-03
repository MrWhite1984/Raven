using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Raven.DB.MinIO;
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
                    if (addContentResponse.IsCanceled)
                    {
                        response.CommentMessage = null;
                        response.Code = 500;
                        response.Message = addContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    else if (addContentResponse.Result.Item2.Equals(null))
                    {
                        response.CommentMessage = null;
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
                    if (addContentResponse.IsCanceled)
                    {
                        response.CommentMessage = null;
                        response.Code = 500;
                        response.Message = addContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    else if (addContentResponse.Result.Item2.Equals(null))
                    {
                        response.CommentMessage = null;
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
            if (dbResponse.IsCanceled)
            {
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
                return Task.FromResult(response);
            }
            else if (dbResponse.Result.Item2 == null)
            {
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
                return Task.FromResult(response);
            }
            else
            {
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
                    if (addCommentContentResponse.IsCanceled)
                    {
                        CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                        response.Code = 500;
                        response.Message = addCommentContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    else if (addCommentContentResponse.Result.Item2 == null)
                    {
                        CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                        response.Code = 500;
                        response.Message = addCommentContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                }
                var getPostResponse = PostExporter.GetPost(Guid.Parse(request.PostId));
                if (getPostResponse.Result.Item2 != "OK" || getPostResponse.Result.Item1 == null)
                {
                    CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                    response.Code = 500;
                    response.Message = getPostResponse.Result.Item2;
                    return Task.FromResult(response);
                }
                getPostResponse.Result.Item1.CommentCount++;
                var updatePostResponse = PostUpdater.UpdatePost(getPostResponse.Result.Item1);
                if (updatePostResponse.Result.Item1 != "OK")
                {
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
                    CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                    response.Code = 500;
                    response.Message = addPostNeo4jResponse.Result;
                    return Task.FromResult(response);
                }
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
                    if (addContentResponse.IsCanceled)
                    {
                        response.CommentMessage = null;
                        response.Code = 500;
                        response.Message = addContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    else if (addContentResponse.Result.Item2.Equals(null))
                    {
                        response.CommentMessage = null;
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
                    if (addContentResponse.IsCanceled)
                    {
                        response.CommentMessage = null;
                        response.Code = 500;
                        response.Message = addContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    else if (addContentResponse.Result.Item2.Equals(null))
                    {
                        response.CommentMessage = null;
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
            if (dbResponse.IsCanceled)
            {
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
                return Task.FromResult(response);
            }
            else if (dbResponse.Result.Item2 == null)
            {
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
                return Task.FromResult(response);
            }
            else
            {
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
                    if (addCommentContentResponse.IsCanceled)
                    {
                        CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                        response.Code = 500;
                        response.Message = addCommentContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    else if (addCommentContentResponse.Result.Item2 == null)
                    {
                        CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                        response.Code = 500;
                        response.Message = addCommentContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                }
                var getCommentResponse = CommentExporter.GetComment(Guid.Parse(request.CommentId));
                if (getCommentResponse.Result.Item2 != "OK" || getCommentResponse.Result.Item1 == null)
                {
                    CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                    response.Code = 500;
                    response.Message = getCommentResponse.Result.Item2;
                    return Task.FromResult(response);
                }
                getCommentResponse.Result.Item1.CommentCount++;
                var updateCommentResponse = CommentUpdater.UpdateComment(getCommentResponse.Result.Item1);
                if (updateCommentResponse.Result.Item1 != "OK")
                {
                    CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                    response.Code = 500;
                    response.Message = updateCommentResponse.Result.Item1;
                    return Task.FromResult(response);
                }
                var getPostResponse = PostExporter.GetPost(Guid.Parse(request.PostId));
                if (getPostResponse.Result.Item2 != "OK" || getPostResponse.Result.Item1 == null)
                {
                    CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                    response.Code = 500;
                    response.Message = getPostResponse.Result.Item2;
                    return Task.FromResult(response);
                }
                getPostResponse.Result.Item1.CommentCount++;
                var updatePostResponse = PostUpdater.UpdatePost(getPostResponse.Result.Item1);
                if (updatePostResponse.Result.Item1 != "OK")
                {
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
                    CommentDeleter.DeleteComment(dbResponse.Result.Item2.Id);
                    response.Code = 500;
                    response.Message = addPostNeo4jResponse.Result;
                    return Task.FromResult(response);
                }
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
            return Task.FromResult(response);
        }

        public override Task<GetCommentsToPostResponse> GetCommentsToPost(GetCommentsToPostRequest request, ServerCallContext context)
        {
            var response = new GetCommentsToPostResponse();
            var neo4jGetCommentsToPostResponse = Neo4jCommentExporter.GetCommentsToPost(request.PostId);
            if (neo4jGetCommentsToPostResponse.Result.Item1 != "OK")
            {
                response.Code = 500;
                response.Message = neo4jGetCommentsToPostResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var dbResponse = CommentExporter.GetCommentsByIdsList(neo4jGetCommentsToPostResponse.Result.Item2);
            if (dbResponse.Result.Item2 != "OK")
            {
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
                return Task.FromResult(response);
            }
            response.Code = 200;
            foreach (var post in dbResponse.Result.Item1)
            {
                var createPostMessageResponse = CreateCommentMessage(post);
                if (createPostMessageResponse.Result.Item2 != "OK")
                {
                    response.Code = 500;
                    response.Message = createPostMessageResponse.Result.Item2;
                    return Task.FromResult(response);
                }
                response.Entities.Add(createPostMessageResponse.Result.Item1);
            }
            response.Message = "OK";
            return Task.FromResult(response);
        }

        public override Task<GetCommentsToCommentResponse> GetCommentsToComment(GetCommentsToCommentRequest request, ServerCallContext context)
        {
            var response = new GetCommentsToCommentResponse();
            var neo4jGetCommentsToPostResponse = Neo4jCommentExporter.GetCommentsToComment(request.CommentId);
            if (neo4jGetCommentsToPostResponse.Result.Item1 != "OK")
            {
                response.Code = 500;
                response.Message = neo4jGetCommentsToPostResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var dbResponse = CommentExporter.GetCommentsByIdsList(neo4jGetCommentsToPostResponse.Result.Item2);
            if (dbResponse.Result.Item2 != "OK")
            {
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
                return Task.FromResult(response);
            }
            response.Code = 200;
            foreach (var post in dbResponse.Result.Item1)
            {
                var createPostMessageResponse = CreateCommentMessage(post);
                if (createPostMessageResponse.Result.Item2 != "OK")
                {
                    response.Code = 500;
                    response.Message = createPostMessageResponse.Result.Item2;
                    return Task.FromResult(response);
                }
                response.Entities.Add(createPostMessageResponse.Result.Item1);
            }
            response.Message = "OK";
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
