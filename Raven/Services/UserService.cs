using Grpc.Core;
using Raven.DB.PSQL.gRPC.Exporters;
using Raven.DB.PSQL.gRPC.Importers;

namespace Raven.Services
{
    public class UserService : UsersHandler.UsersHandlerBase
    {
        public override Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            CreateUserResponse response = new CreateUserResponse();
            var dbResponse = UserImporter.CreateUser(new DB.PSQL.Entity.Users
            {
                Id = request.UserId,
            });
            if (dbResponse.IsCanceled)
            {
                response.User = null;
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
            }
            else if (dbResponse.Result.Item2 == null)
            {
                response.User = "";
                response.Code = 500;
                response.Message = dbResponse.Result.Item1;
            }
            else
            {
                response.User = dbResponse.Result.Item2.Id;
                response.Code = 200;
                response.Message += dbResponse.Result.Item1;
            }
            return Task.FromResult(response);
        }

        public override Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
        {
            GetUsersResponse response = new GetUsersResponse();
            var dbResponse = UserExporter.GetUsersList();
            if (dbResponse.IsCanceled)
            {
                response.Entities.Add(new List<string>());
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
            }
            else if (dbResponse.Result.Item2 != "OK")
            {
                response.Entities.Add(new List<string>());
                response.Code = 500;
                response.Message = dbResponse.Result.Item2;
            }
            else
            {
                foreach (var user in dbResponse.Result.Item1)
                    response.Entities
                        .Add(user.Id);

                response.Code = 200;
                response.Message = dbResponse.Result.Item2;
            }
            return Task.FromResult(response);
        }
    }
}
