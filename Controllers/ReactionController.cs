using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Эмодзи-реакции на посты (как в Instagram: 😍🔥😂👍😮😢).
/// Одна реакция на пользователя+пост; повторный вызов меняет эмодзи.
/// userId передаётся явно (у доп-бэкенда нет авторизации).
/// </summary>
[ApiController]
[Route("Reaction")]
public class ReactionController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReactionController(AppDbContext db) => _db = db;

    public record ReactDto(string UserId, string UserName, int PostId, string Emoji);
    public record ReactionSummary(string Emoji, int Count);

    /// <summary>Поставить/сменить реакцию.</summary>
    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] ReactDto dto)
    {
        var existing = await _db.Reactions.FirstOrDefaultAsync(r => r.UserId == dto.UserId && r.PostId == dto.PostId);
        if (existing is null)
            _db.Reactions.Add(new Reaction { UserId = dto.UserId, UserName = dto.UserName, PostId = dto.PostId, Emoji = dto.Emoji });
        else
        {
            existing.Emoji = dto.Emoji;
            existing.CreatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>Убрать свою реакцию с поста.</summary>
    [HttpDelete("remove")]
    public async Task<IActionResult> Remove([FromQuery] string userId, [FromQuery] int postId)
    {
        var existing = await _db.Reactions.FirstOrDefaultAsync(r => r.UserId == userId && r.PostId == postId);
        if (existing is not null)
        {
            _db.Reactions.Remove(existing);
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>Реакции поста: список + сводка по эмодзи.</summary>
    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] int postId, [FromQuery] string? userId)
    {
        var reactions = await _db.Reactions.Where(r => r.PostId == postId).ToListAsync();
        var summary = reactions
            .GroupBy(r => r.Emoji)
            .Select(g => new ReactionSummary(g.Key, g.Count()))
            .OrderByDescending(s => s.Count)
            .ToList();
        var mine = userId is null ? null : reactions.FirstOrDefault(r => r.UserId == userId)?.Emoji;
        return Ok(ApiResponse<object>.Ok(new { total = reactions.Count, summary, mine, reactions }));
    }
}
