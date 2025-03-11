using Grpc.Core;
using Raven.DB.PSQL.gRPC.Exporters;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Raven.DB.PSQL.TrainingData;
using Raven.DB.PSQL.TrainingData.Entity;
using System.Text.RegularExpressions;

namespace Raven.Services
{
    public class TagsPredictorService : TagsPredictorHandler.TagsPredictorHandlerBase
    {
        public override Task<RetrainTagsPredictorModelResponse> RetrainTagsPredictorModel(RetrainTagsPredictorModelRequest request, ServerCallContext context)
        {
            try
            {
                HttpClient client = new HttpClient();
                using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, "http://raven-tags-predictor:8008/retrain");
                client.SendAsync(httpRequest);
                Logger.Logger.Log(LogLevel.Information, "Переобучение модели запущено вручную");
                return Task.FromResult(new RetrainTagsPredictorModelResponse { Code = 200, Message = "OK" });
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LogLevel.Error, ex.Message);
                return Task.FromResult(new RetrainTagsPredictorModelResponse { Code=500, Message = ex.Message });
            }
        }

        public override Task<PredictTagsResponse> PredictTags(PredictTagsRequest request, ServerCallContext context)
        {
            try
            {
                HttpClient client = new HttpClient();
                var postData = new
                {
                    body = request.BodyPost
                };
                string postDataJson = JsonSerializer.Serialize(postData);
                var content = new StringContent(postDataJson, Encoding.UTF8, "application/json");
                var httpResponse = client.PostAsync("http://raven-tags-predictor:8008/predict", content);
                httpResponse.Result.EnsureSuccessStatusCode();
                var responseBody = httpResponse.Result.Content.ReadAsStringAsync();
                var tagsNames = JsonSerializer.Deserialize<List<List<string>>>(responseBody.Result);
                var tags = TagExporter.GetTagsByNames(tagsNames.SelectMany(o=>o).ToList());
                var response = new PredictTagsResponse() { Code = 200, Message = "OK"};
                foreach (var tag in tags.Result.Item1)
                {
                    response.TagMessage.Add(new TagMessage() { Id = (uint)tag.Id, Name = tag.Name });
                }
                Logger.Logger.Log(LogLevel.Information, $"Подобрано {response.TagMessage.Count} тегов к посту");
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LogLevel.Error, ex.Message);
                return Task.FromResult(new PredictTagsResponse() { Code=500, Message = ex.Message });
            }
        }

        public override Task<AddBodyPostWithTagsToTrainDataResponse> AddBodyPostWithTagsToTrainData(AddBodyPostWithTagsToTrainDataRequest request, ServerCallContext context)
        {
            try
            {
                var response = new AddBodyPostWithTagsToTrainDataResponse();
                var dbResponse = Importer.AddDataToTrainingDb(new Post() 
                { 
                    Body = Regex.Replace(request.BodyPost, @"\[[^\]]*\]", "") 
                }, 
                    request.TagsNames.ToList());
                if (dbResponse.Result != "OK")
                    throw new Exception(dbResponse.Result.ToString());
                response.Code = 200;
                response.Message = "OK";
                Logger.Logger.Log(LogLevel.Information, "Добавлен пост для переобучения модели");
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LogLevel.Error, ex.Message);
                return Task.FromResult(new AddBodyPostWithTagsToTrainDataResponse() 
                { Code = 500, Message = ex.Message });
            }
        }
    }
}
