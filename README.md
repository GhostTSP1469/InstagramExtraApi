# InstagramExtraApi — дополнительный бэкенд

Компаньон к основному API `instagram-api.softclub.tj`. Сюда выносим ручки,
которые **сломаны или отсутствуют** в основном бэкенде, а также новые идеи.

Стек: **ASP.NET Core 8 (C#) + EF Core + SQLite**. Формат ответов 1:1 как в
основном API (`{ data, errors, statusCode }`), роуты те же — поэтому фронт
переключается на этот бэк просто сменой базового адреса.

## Что уже реализовано

### Location (в основном API `update-Location` падает с 400 — тут работает)
| Метод | Роут | Описание |
|-------|------|----------|
| GET | `/Location/get-Locations` | список с фильтрами и пагинацией |
| GET | `/Location/get-Location-by-id?id=` | одна локация |
| POST | `/Location/add-Location` | создать |
| PUT | `/Location/update-Location` | **обновить (рабочее)** |
| DELETE | `/Location/delete-Location?id=` | удалить |

Проверено вживую: create → update → get → delete отрабатывают, `update`
возвращает обновлённый объект.

## Запуск локально

```bash
dotnet run
```

- API: `http://localhost:5xxx` (порт покажет консоль)
- Swagger: `http://localhost:5xxx/swagger`
- health: `GET /`

БД (`app.db`, SQLite) создаётся автоматически при первом старте и наполняется
стартовыми локациями.

## Деплой

Есть `Dockerfile`. Сервис читает порт из переменной окружения `PORT`
(Render / Railway / Fly и т.п. её прокидывают), путь к БД — из `DB_PATH`.

```bash
docker build -t instagram-extra-api .
docker run -p 8080:8080 -e PORT=8080 instagram-extra-api
```

Без Docker (на хосте с .NET 8):

```bash
dotnet publish -c Release -o out
PORT=8080 dotnet out/InstagramExtraApi.dll
```

## Как подключить с фронта

Базовый адрес API-клиента переопределяется переменной `NEXT_PUBLIC_API_URL`.
Чтобы локации ходили в этот бэк, а остальное — в основной, можно завести
второй инстанс клиента с этим `baseUrl` (или временно переключить весь клиент).

## Как добавлять новые ручки

1. Модель → `Models/`, при необходимости DTO → `Dtos/`.
2. `DbSet<...>` в `Data/AppDbContext.cs` (+ сид при желании).
3. Контроллер → `Controllers/`, роут в стиле основного API, ответ через
   `ApiResponse<T>` / `PagedResponse<T>` из `Common/`.
