using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Identity.Data;
using Raven.DB.MinIO;
using Raven.DB.Neo4j.Importers;
using Raven.DB.PSQL.Entity;
using Raven.DB.PSQL.Entity.@enum;
using Raven.DB.PSQL.gRPC.Deleters;
using Raven.DB.PSQL.gRPC.Exporters;
using Raven.DB.PSQL.gRPC.Importers;

namespace Raven.Services
{
    public class PostService : PostHandler.PostHandlerBase
    {
        public override Task<CreatePostResponse> CreatePost(CreatePostRequest request, ServerCallContext context)
        {
            CreatePostResponse response = new CreatePostResponse();
            var getCategoryResponse = CategoryExporter.GetCategory((int)request.CategoryId);
            if (getCategoryResponse.Result.Item1 == null)
            {
                response.PostMessage = null;
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
                    response.PostMessage = null;
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
                    if (addContentResponse.IsCanceled)
                    {
                        response.PostMessage = null;
                        response.Code = 500;
                        response.Message = addContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    else if (addContentResponse.Result.Item2.Equals(null))
                    {
                        response.PostMessage = null;
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
                    if (addContentResponse.IsCanceled)
                    {
                        response.PostMessage = null;
                        response.Code = 500;
                        response.Message = addContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    else if (addContentResponse.Result.Item2.Equals(null))
                    {
                        response.PostMessage = null;
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
                    response.PostMessage = null;
                    response.Code = 500;
                    response.Message = getTagResponse.Result.Item2;
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
            if (dbResponse.IsCanceled)
            {
                response.PostMessage = null;
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
                return Task.FromResult(response);
            }
            else if (dbResponse.Result.Item2 == null)
            {
                response.PostMessage = null;
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
                    if (createTagMessageResponse.IsCanceled)
                    {
                        PostDeleter.DeletePost(dbResponse.Result.Item2.Id);
                        response.PostMessage = null;
                        response.Code = 500;
                        response.Message = createTagMessageResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    else if (createTagMessageResponse.Result.Item2 == null)
                    {
                        PostDeleter.DeletePost(dbResponse.Result.Item2.Id);
                        response.PostMessage = null;
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
                    if (addPostContentResponse.IsCanceled)
                    {
                        PostDeleter.DeletePost(dbResponse.Result.Item2.Id);
                        response.PostMessage = null;
                        response.Code = 500;
                        response.Message = addPostContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    else if (addPostContentResponse.Result.Item2 == null)
                    {
                        PostDeleter.DeletePost(dbResponse.Result.Item2.Id);
                        response.PostMessage = null;
                        response.Code = 500;
                        response.Message = addPostContentResponse.Result.Item1;
                        return Task.FromResult(response);
                    }
                    var addPostNeo4jResponse = Neo4jPostImporter.AddNewPost(dbResponse.Result.Item2.Id.ToString());
                    if (addPostNeo4jResponse.Result != "OK")
                    {
                        PostDeleter.DeletePost(dbResponse.Result.Item2.Id);
                        response.PostMessage = null;
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
                    PostCount = (uint)getCategoryResponse.Result.Item1.PostCount,
                    Image = getCategoryImageResponse != null ? ByteString.CopyFrom(getCategoryImageResponse.Result.Item2) : ByteString.Empty
                },
                AuthorId = dbResponse.Result.Item2.AuthorId,
                CreatedAt = Timestamp.FromDateTime(dbResponse.Result.Item2.CreatedAt)
            };
            response.PostMessage.PostContentMessage.AddRange(request.PostContentMessage);
            response.PostMessage.Tags.AddRange(tagMessages);
            response.Code = 200;
            response.Message = "OK";
            return Task.FromResult(response);
        }

        public override Task<AddPostToLikedResponse> AddPostToLiked(AddPostToLikedRequest request, ServerCallContext context)
        {
            var response = new AddPostToLikedResponse();
            var neo4jResponse = Neo4jRelationshipImporter.AddPostLikeRelationship(request.UserId, request.PostId).Result;
            if(neo4jResponse != "OK")
            {
                response.Code = 500;
                response.Message = neo4jResponse;
            }
            else
            {
                response.Code = 200;
                response.Message = neo4jResponse;
            }
            return Task.FromResult(response);
        }

        public override Task<AddPostToViewsResponse> AddPostToViews(AddPostToViewsRequest request, ServerCallContext context)
        {
            var response = new AddPostToViewsResponse();
            var neo4jResponse = Neo4jRelationshipImporter.AddPostViewRelationship(request.UserId, request.PostId).Result;
            if (neo4jResponse != "OK")
            {
                response.Code = 500;
                response.Message = neo4jResponse;
            }
            else
            {
                response.Code = 200;
                response.Message = neo4jResponse;
            }
            return Task.FromResult(response);
        }

        public override Task<AddPostToBookmarksResponse> AddPostToBookmarks(AddPostToBookmarksRequest request, ServerCallContext context)
        {
            var response = new AddPostToBookmarksResponse();
            var neo4jResponse = Neo4jRelationshipImporter.AddPostBookmarkRelationship(request.UserId, request.PostId).Result;
            if (neo4jResponse != "OK")
            {
                response.Code = 500;
                response.Message = neo4jResponse;
            }
            else
            {
                response.Code = 200;
                response.Message = neo4jResponse;
            }
            return Task.FromResult(response);
        }
    }
}
