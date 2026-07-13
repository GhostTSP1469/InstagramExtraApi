using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Репосты — пользователь делится чужим постом у себя (как «поделиться» в Instagram).
/// Такого в основном API нет. Один репост на пользователя+пост; повторный вызов
/// обновляет подпись. userId передаётся явно (у доп-бэкенда нет авторизации).
/// </summary>
[ApiController]
[Route("Repost")]
public class RepostController : ControllerBase
{
    private readonly AppDbContext _db;
    public RepostController(AppDbContext db) => _db = db;

    public record RepostDto(
        string UserId,
        string UserName,
        int PostId,
        string OriginalAuthorId,
        string OriginalAuthorName,
        string? Caption);

    /// <summary>Сделать/обновить репост поста.</summary>
    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] RepostDto dto)
    {
        var existing = await _db.Reposts.FirstOrDefaultAsync(r => r.UserId == dto.UserId && r.PostId == dto.PostId);
        if (existing is null)
        {
            var repost = new Repost
            {
                UserId = dto.UserId,
                UserName = dto.UserName,
                PostId = dto.PostId,
                OriginalAuthorId = dto.OriginalAuthorId,
                OriginalAuthorName = dto.OriginalAuthorName,
                Caption = dto.Caption ?? string.Empty,
            };
            _db.Reposts.Add(repost);
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<Repost>.Ok(repost));
        }
        existing.Caption = dto.Caption ?? string.Empty;
        existing.CreatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Repost>.Ok(existing));
    }

    /// <summary>Убрать свой репост поста.</summary>
    [HttpDelete("remove")]
    public async Task<IActionResult> Remove([FromQuery] string userId, [FromQuery] int postId)
    {
        var existing = await _db.Reposts.FirstOrDefaultAsync(r => r.UserId == userId && r.PostId == postId);
        if (existing is not null)
        {
            _db.Reposts.Remove(existing);
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>Репосты конкретного поста: счётчик + список + репостил ли текущий пользователь.</summary>
    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] int postId, [FromQuery] string? userId)
    {
        var reposts = await _db.Reposts
            .Where(r => r.PostId == postId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        var mine = userId is not null && reposts.Any(r => r.UserId == userId);
        return Ok(ApiResponse<object>.Ok(new { total = reposts.Count, mine, reposts }));
    }

    /// <summary>Лента репостов пользователя (что он у себя расшарил).</summary>
    [HttpGet("user")]
    public async Task<IActionResult> ByUser([FromQuery] string userId)
    {
        var reposts = await _db.Reposts
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(50)
            .ToListAsync();
        return Ok(ApiResponse<List<Repost>>.Ok(reposts));
    }
}
