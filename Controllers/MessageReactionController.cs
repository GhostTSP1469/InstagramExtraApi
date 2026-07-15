using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Реакции на сообщения (эмодзи/стикер). Одна реакция на пользователя+сообщение,
/// повторный вызов меняет эмодзи. messageId — id из основного API или ExtraMessage.
/// </summary>
[ApiController]
[Route("MessageReaction")]
public class MessageReactionController : ControllerBase
{
    private readonly AppDbContext _db;
    public MessageReactionController(AppDbContext db) => _db = db;

    public record ReactDto(int MessageId, string UserId, string UserName, string Emoji);
    public record ReactionSummary(string Emoji, int Count);

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] ReactDto dto)
    {
        var existing = await _db.MessageReactions.FirstOrDefaultAsync(r => r.MessageId == dto.MessageId && r.UserId == dto.UserId);
        if (existing is null)
            _db.MessageReactions.Add(new MessageReaction { MessageId = dto.MessageId, UserId = dto.UserId, UserName = dto.UserName, Emoji = dto.Emoji });
        else
        {
            existing.Emoji = dto.Emoji;
            existing.CreatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> Remove([FromQuery] int messageId, [FromQuery] string userId)
    {
        var row = await _db.MessageReactions.FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId);
        if (row is not null)
        {
            _db.MessageReactions.Remove(row);
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>Реакции сообщения: сводка + моя реакция.</summary>
    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] int messageId, [FromQuery] string? userId)
    {
        var all = await _db.MessageReactions.Where(r => r.MessageId == messageId).ToListAsync();
        var summary = all.GroupBy(r => r.Emoji)
            .Select(g => new ReactionSummary(g.Key, g.Count()))
            .OrderByDescending(s => s.Count).ToList();
        var mine = userId is null ? null : all.FirstOrDefault(r => r.UserId == userId)?.Emoji;
        return Ok(ApiResponse<object>.Ok(new { total = all.Count, summary, mine, reactions = all }));
    }
}
