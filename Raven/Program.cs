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
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

// Configure the HTTP request pipeline.
app.MapGrpcService<CategoryService>();
app.MapGrpcService<TagService>();
app.MapGrpcService<UserService>();
app.MapGrpcService<CommentService>();
app.MapGrpcService<PostService>();
app.MapRazorPages();
app.MapGet("/", () => "gRPC запущен\nWeb-интерфейс доступен на /Logs");

app.Run();
