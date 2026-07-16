using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Взаимодействия со сторис ОСНОВНОГО API (softclub) — реакции, ответы и
/// кросс-девайс «просмотрено». Всё привязано к storyId основного API, так что
/// заменяет фейки (реакции через /Reaction и ответы через коммент к посту).
/// Авторизации у доп-бэка нет — userId передаётся явно.
/// </summary>
[ApiController]
[Route("StoryInteract")]
public class StoryInteractionController : ControllerBase
{
    private readonly AppDbContext _db;
    public StoryInteractionController(AppDbContext db) => _db = db;

    public record StoryReactRequest(int StoryId, string UserId, string UserName, string Emoji);
    public record StoryReplyRequest(int StoryId, string OwnerUserId, string FromUserId, string FromUserName, string Text);
    public record StoryReactionSummary(string Emoji, int Count);

    // ── реакции (#4) ──────────────────────────────────────────────────────────

    /// <summary>Поставить/сменить реакцию на сторис.</summary>
    [HttpPost("react")]
    public async Task<IActionResult> React([FromBody] StoryReactRequest dto)
    {
        var existing = await _db.StoryReactions.FirstOrDefaultAsync(r => r.StoryId == dto.StoryId && r.UserId == dto.UserId);
        if (existing is null)
            _db.StoryReactions.Add(new StoryReaction { StoryId = dto.StoryId, UserId = dto.UserId, UserName = dto.UserName, Emoji = dto.Emoji });
        else { existing.Emoji = dto.Emoji; existing.CreatedAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>Реакции сторис: сводка по эмодзи + моя реакция.</summary>
    [HttpGet("get-reactions")]
    public async Task<IActionResult> GetReactions([FromQuery] int storyId, [FromQuery] string? userId)
    {
        var all = await _db.StoryReactions.Where(r => r.StoryId == storyId).ToListAsync();
        var summary = all.GroupBy(r => r.Emoji)
            .Select(g => new StoryReactionSummary(g.Key, g.Count()))
            .OrderByDescending(s => s.Count).ToList();
        var mine = userId is null ? null : all.FirstOrDefault(r => r.UserId == userId)?.Emoji;
        return Ok(ApiResponse<object>.Ok(new { total = all.Count, summary, mine, reactions = all }));
    }

    [HttpDelete("remove-reaction")]
    public async Task<IActionResult> RemoveReaction([FromQuery] int storyId, [FromQuery] string userId)
    {
        var row = await _db.StoryReactions.FirstOrDefaultAsync(r => r.StoryId == storyId && r.UserId == userId);
        if (row is not null) { _db.StoryReactions.Remove(row); await _db.SaveChangesAsync(); }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    // ── ответы (#5) ───────────────────────────────────────────────────────────

    /// <summary>Ответить на сторис (доставляется автору).</summary>
    [HttpPost("reply")]
    public async Task<IActionResult> Reply([FromBody] StoryReplyRequest dto)
    {
        var reply = new StoryReply
        {
            StoryId = dto.StoryId,
            OwnerUserId = dto.OwnerUserId,
            FromUserId = dto.FromUserId,
            FromUserName = dto.FromUserName,
            Text = dto.Text,
        };
        _db.StoryReplies.Add(reply);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<StoryReply>.Ok(reply));
    }

    /// <summary>Ответы, пришедшие мне как автору сторис (или ответы конкретной сторис).</summary>
    [HttpGet("get-replies")]
    public async Task<IActionResult> GetReplies([FromQuery] string? ownerUserId, [FromQuery] int? storyId)
    {
        var q = _db.StoryReplies.AsQueryable();
        if (!string.IsNullOrWhiteSpace(ownerUserId)) q = q.Where(r => r.OwnerUserId == ownerUserId);
        if (storyId is not null) q = q.Where(r => r.StoryId == storyId);
        var items = await q.OrderByDescending(r => r.Id).Take(100).ToListAsync();
        return Ok(ApiResponse<List<StoryReply>>.Ok(items));
    }

    // ── просмотрено / isViewed (#1) ───────────────────────────────────────────

    /// <summary>Отметить сторис просмотренной (кросс-девайс).</summary>
    [HttpPost("mark-viewed")]
    public async Task<IActionResult> MarkViewed([FromQuery] int storyId, [FromQuery] string userId)
    {
        var seen = await _db.StorySeen.AnyAsync(s => s.StoryId == storyId && s.UserId == userId);
        if (!seen)
        {
            _db.StorySeen.Add(new StorySeen { StoryId = storyId, UserId = userId });
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>Список storyId, которые пользователь уже видел (для серой обводки).</summary>
    [HttpGet("get-viewed")]
    public async Task<IActionResult> GetViewed([FromQuery] string userId)
    {
        var ids = await _db.StorySeen.Where(s => s.UserId == userId).Select(s => s.StoryId).ToListAsync();
        return Ok(ApiResponse<List<int>>.Ok(ids));
    }
}
