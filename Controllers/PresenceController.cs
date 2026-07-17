using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Присутствие «в сети»: клиент периодически шлёт heartbeat, а чат спрашивает
/// статусы собеседников. Онлайн = пинговал за последние 60 секунд. userId
/// передаётся явно (авторизации у доп-бэка нет).
/// </summary>
[ApiController]
[Route("Presence")]
public class PresenceController : ControllerBase
{
    private readonly AppDbContext _db;
    public PresenceController(AppDbContext db) => _db = db;

    private static readonly TimeSpan OnlineWindow = TimeSpan.FromSeconds(60);

    public record PresenceDto(string UserId, bool Online, DateTime LastSeenAt);

    /// <summary>«Я онлайн» — клиент шлёт это каждые ~30с, пока открыт.</summary>
    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Ok(ApiResponse<bool>.Fail("userId required", 400));

        var row = await _db.Presences.FirstOrDefaultAsync(p => p.UserId == userId);
        if (row is null)
        {
            row = new Presence { UserId = userId };
            _db.Presences.Add(row);
        }
        row.LastSeenAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>Статусы для списка userId (через запятую).</summary>
    [HttpGet("status")]
    public async Task<IActionResult> Status([FromQuery] string userIds)
    {
        var ids = (userIds ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToList();

        var rows = await _db.Presences.Where(p => ids.Contains(p.UserId)).ToListAsync();
        var byId = rows.ToDictionary(r => r.UserId);
        var now = DateTime.UtcNow;

        var result = ids.Select(id =>
            byId.TryGetValue(id, out var r)
                ? new PresenceDto(id, now - r.LastSeenAt <= OnlineWindow, r.LastSeenAt)
                : new PresenceDto(id, false, DateTime.MinValue)
        ).ToList();

        return Ok(ApiResponse<List<PresenceDto>>.Ok(result));
    }
}
