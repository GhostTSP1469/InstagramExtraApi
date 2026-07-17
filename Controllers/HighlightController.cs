using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Highlights — закреплённые подборки сторис на профиле. Медиа берём из сторис
/// основного API (имена файлов), поэтому хранится здесь. userId передаётся явно.
/// </summary>
[ApiController]
[Route("Highlight")]
public class HighlightController : ControllerBase
{
    private readonly AppDbContext _db;
    public HighlightController(AppDbContext db) => _db = db;

    public record ItemDto(string MediaUrl, string? Type);
    public record CreateDto(string UserId, string Title, string? CoverUrl, List<ItemDto> Items);
    public record HighlightDto(int Id, string UserId, string Title, string? CoverUrl, DateTime CreatedAt, List<HighlightItem> Items);

    /// <summary>Создать подборку из выбранных медиа.</summary>
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateDto dto)
    {
        var h = new Highlight
        {
            UserId = dto.UserId,
            Title = string.IsNullOrWhiteSpace(dto.Title) ? "Highlights" : dto.Title,
            CoverUrl = dto.CoverUrl ?? dto.Items?.FirstOrDefault()?.MediaUrl,
        };
        _db.Highlights.Add(h);
        await _db.SaveChangesAsync();

        foreach (var it in dto.Items ?? new())
            _db.HighlightItems.Add(new HighlightItem { HighlightId = h.Id, MediaUrl = it.MediaUrl, Type = it.Type == "video" ? "video" : "image" });
        await _db.SaveChangesAsync();

        var items = await _db.HighlightItems.Where(i => i.HighlightId == h.Id).ToListAsync();
        return Ok(ApiResponse<HighlightDto>.Ok(new HighlightDto(h.Id, h.UserId, h.Title, h.CoverUrl, h.CreatedAt, items)));
    }

    /// <summary>Подборки пользователя (вместе с их элементами).</summary>
    [HttpGet("by-user")]
    public async Task<IActionResult> ByUser([FromQuery] string userId)
    {
        var hs = await _db.Highlights.Where(h => h.UserId == userId).OrderByDescending(h => h.Id).ToListAsync();
        var ids = hs.Select(h => h.Id).ToList();
        var items = await _db.HighlightItems.Where(i => ids.Contains(i.HighlightId)).ToListAsync();
        var dtos = hs.Select(h => new HighlightDto(h.Id, h.UserId, h.Title, h.CoverUrl, h.CreatedAt,
            items.Where(i => i.HighlightId == h.Id).ToList())).ToList();
        return Ok(ApiResponse<List<HighlightDto>>.Ok(dtos));
    }

    [HttpPost("add-item")]
    public async Task<IActionResult> AddItem([FromBody] HighlightItem item)
    {
        item.Id = 0;
        item.Type = item.Type == "video" ? "video" : "image";
        _db.HighlightItems.Add(item);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<HighlightItem>.Ok(item));
    }

    [HttpDelete("remove-item")]
    public async Task<IActionResult> RemoveItem([FromQuery] int itemId)
    {
        var it = await _db.HighlightItems.FindAsync(itemId);
        if (it is not null) { _db.HighlightItems.Remove(it); await _db.SaveChangesAsync(); }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        var h = await _db.Highlights.FindAsync(id);
        if (h is not null)
        {
            var items = _db.HighlightItems.Where(i => i.HighlightId == id);
            _db.HighlightItems.RemoveRange(items);
            _db.Highlights.Remove(h);
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
