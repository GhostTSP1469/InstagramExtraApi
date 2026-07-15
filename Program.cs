using InstagramExtraApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// БД: SQLite (файл app.db). Путь можно переопределить через DB_PATH.
// Если в пути есть папка (напр. /data/app.db на постоянном диске) — создаём её.
// Если создать нельзя (диск не подключён/нет прав) — откатываемся на локальный app.db,
// чтобы деплой не падал с "unable to open database file".
var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "app.db";
try
{
    var dbDir = Path.GetDirectoryName(Path.GetFullPath(dbPath));
    if (!string.IsNullOrEmpty(dbDir))
        Directory.CreateDirectory(dbDir);
}
catch
{
    dbPath = "app.db";
}
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient для прокси к Giphy (GIF в чате).
builder.Services.AddHttpClient();

// Папка загрузок создаётся ДО билда хоста, чтобы WebRootPath (wwwroot) существовал
// и UseStaticFiles/сохранение файлов работали.
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads"));

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

// Раздача загруженных файлов (голосовые/видео/файлы чата, сторис) из /uploads.
app.UseStaticFiles();

// Swagger доступен всегда (в т.ч. на проде) — так удобнее тестировать после деплоя.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.MapControllers();

app.Run();
