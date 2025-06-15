global using Backend.Models;
global using Backend.Helpers;

using Backend;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<GameService>();
builder.Services.AddControllers();
builder.Services.AddCors(_ =>
    _.AddDefaultPolicy(p =>
    {
        p.AllowAnyMethod();
        p.SetIsOriginAllowed(_ => true);
        p.AllowAnyHeader();
    })
);

var app = builder.Build();
app.MapControllers();
app.UseCors();
await app.RunAsync();