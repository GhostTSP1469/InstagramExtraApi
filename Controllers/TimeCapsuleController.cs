using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// «Капсула времени»: пост прячется, пока не наступит RevealAt. Основной API поля
/// с датой раскрытия не имеет, поэтому храним связь здесь, а клиент блюрит/гейтит
/// пост до даты. userId (владелец) передаётся явно (авторизации у доп-бэка нет).
/// </summary>
[ApiController]
[Route("TimeCapsule")]
public class TimeCapsuleController : ControllerBase
{
    private readonly AppDbContext _db;
    public TimeCapsuleController(AppDbContext db) => _db = db;

    public record SetRequest(int PostId, string UserId, DateTime RevealAt);
    public record CapsuleDto(int PostId, string UserId, DateTime RevealAt, bool Locked);

    private static DateTime AsUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime();

    private static CapsuleDto ToDto(TimeCapsule c) =>
        new(c.PostId, c.UserId, c.RevealAt, c.RevealAt > DateTime.UtcNow);

    /// <summary>Поставить/обновить капсулу на пост (раскроется в RevealAt, UTC).</summary>
    [HttpPost("set")]
    public async Task<IActionResult> Set([FromBody] SetRequest dto)
    {
        if (dto.PostId <= 0 || string.IsNullOrWhiteSpace(dto.UserId))
            return Ok(ApiResponse<CapsuleDto>.Fail("postId and userId required", 400));

        var row = await _db.TimeCapsules.FirstOrDefaultAsync(c => c.PostId == dto.PostId);
        if (row is null)
        {
            row = new TimeCapsule { PostId = dto.PostId };
            _db.TimeCapsules.Add(row);
        }
        row.UserId = dto.UserId;
        row.RevealAt = AsUtc(dto.RevealAt);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<CapsuleDto>.Ok(ToDto(row)));
    }

    /// <summary>Снять капсулу с поста.</summary>
    [HttpDelete("remove")]
    public async Task<IActionResult> Remove([FromQuery] int postId)
    {
        var row = await _db.TimeCapsules.FirstOrDefaultAsync(c => c.PostId == postId);
        if (row is not null)
        {
            _db.TimeCapsules.Remove(row);
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>Все капсулы — клиент прячет посты, у которых RevealAt ещё в будущем.</summary>
    [HttpGet("all")]
    public async Task<IActionResult> All()
    {
        var rows = await _db.TimeCapsules.ToListAsync();
        return Ok(ApiResponse<List<CapsuleDto>>.Ok(rows.Select(ToDto).ToList()));
    }
}
