using DocumentsFillerAPI;
using DocumentsFillerAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
var app = builder.Build();

app.UseMiddleware<AuthMiddleware>();
// Configure the HTTP request pipeline.

app.UseRouting();

app.MapControllers();
ConfigProvider.Configuration = app.Configuration;

app.Run();