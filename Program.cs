using InstagramExtraApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// БД: SQLite (файл app.db). Путь можно переопределить через DB_PATH.
var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "app.db";
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS — фронт (Next) должен свободно ходить сюда.
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Деплой: многие хостинги передают порт через переменную PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Создаём БД и накатываем сид при старте.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Swagger доступен всегда (в т.ч. на проде) — так удобнее тестировать после деплоя.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.MapControllers();

app.Run();
