using Grpc.Core;
using Raven.DB.Neo4j.Importers;
using Raven.DB.PSQL.gRPC.Deleters;
using Raven.DB.PSQL.gRPC.Exporters;
using Raven.DB.PSQL.gRPC.Importers;

namespace Raven.Services
{
    public class UserService : UserHandler.UserHandlerBase
    {
        public override Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            CreateUserResponse response = new CreateUserResponse();
            var dbResponse = UserImporter.CreateUser(new DB.PSQL.Entity.Users
            {
                Id = request.UserId,
            });
            if (dbResponse.Result.Item2 == null)
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item1);
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
                return Task.FromResult(response);
            }
            var neo4jResponse = Neo4jUserImporter.AddNewUser(dbResponse.Result.Item2.Id).Result;
            if (neo4jResponse != "OK")
            {
                UserDeleter.DeleteUser(dbResponse.Result.Item2.Id);
                Logger.Logger.Log(LogLevel.Error, neo4jResponse);
                response.Code = 500;
                response.Message += neo4jResponse;
                return Task.FromResult(response);
            }
            response.User = dbResponse.Result.Item2.Id;
            response.Code = 200;
            response.Message += dbResponse.Result.Item1;
            Logger.Logger.Log(LogLevel.Information, $"Пользователь {response.User} добавлен в базу данных");
            return Task.FromResult(response);
        }

        public override Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
        {
            GetUsersResponse response = new GetUsersResponse();
            var dbResponse = UserExporter.GetUsersList();
            if (dbResponse.Result.Item2 != "OK")
            {
                Logger.Logger.Log(LogLevel.Error, dbResponse.Result.Item2);
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
                return Task.FromResult(response);
            }
            foreach (var user in dbResponse.Result.Item1)
                response.Entities
                    .Add(user.Id);

            response.Code = 200;
            response.Message = dbResponse.Result.Item2;
            Logger.Logger
                .Log(LogLevel.Information, $"Выполнен запрос на получение пользователей ({response.Entities.Count})");
            return Task.FromResult(response);
        }
    }
}
