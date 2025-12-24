global using Backend.Models;
global using Backend.Helpers;

using Backend;
using Backend.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<GameService>();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(_ =>
    _.AddDefaultPolicy(p =>
    {
        p.AllowAnyMethod();
        p.SetIsOriginAllowed(_ => true);
        p.AllowAnyHeader();
        p.AllowCredentials();
    })
);

var app = builder.Build();
app.MapControllers();
app.MapHub<GameHub>("/gameHub");
app.UseCors();
await app.RunAsync();