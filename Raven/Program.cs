using Raven.BackgroundServices;
using Raven.DB.MinIO;
using Raven.DB.Neo4j;
using Raven.Services;

var minio = new MinioContext();
minio.EnsureBucketsExist();
Neo4jContext.InitRequests();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddHostedService<LogsDropper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<CategoryService>();
app.MapGrpcService<TagService>();
app.MapGrpcService<UserService>();
app.MapGrpcService<CommentService>();
app.MapGrpcService<PostService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
