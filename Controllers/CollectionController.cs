using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Saved collections — папки сохранённых постов. Хранят postId постов основного
/// API; клиент фильтрует свои избранные по этим id. userId передаётся явно.
/// </summary>
[ApiController]
[Route("Collection")]
public class CollectionController : ControllerBase
{
    private readonly AppDbContext _db;
    public CollectionController(AppDbContext db) => _db = db;

    public record CreateDto(string UserId, string Name, string? CoverUrl, List<int>? PostIds);
    public record CollectionDto(int Id, string UserId, string Name, string? CoverUrl, DateTime CreatedAt, List<int> PostIds);

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateDto dto)
    {
        var c = new Collection
        {
            UserId = dto.UserId,
            Name = string.IsNullOrWhiteSpace(dto.Name) ? "Collection" : dto.Name,
            CoverUrl = dto.CoverUrl,
        };
        _db.Collections.Add(c);
        await _db.SaveChangesAsync();

        foreach (var pid in (dto.PostIds ?? new()).Distinct())
            _db.CollectionItems.Add(new CollectionItem { CollectionId = c.Id, PostId = pid });
        await _db.SaveChangesAsync();

        var ids = await _db.CollectionItems.Where(i => i.CollectionId == c.Id).Select(i => i.PostId).ToListAsync();
        return Ok(ApiResponse<CollectionDto>.Ok(new CollectionDto(c.Id, c.UserId, c.Name, c.CoverUrl, c.CreatedAt, ids)));
    }

    /// <summary>Коллекции пользователя вместе с их postId.</summary>
    [HttpGet("by-user")]
    public async Task<IActionResult> ByUser([FromQuery] string userId)
    {
        var cols = await _db.Collections.Where(c => c.UserId == userId).OrderByDescending(c => c.Id).ToListAsync();
        var ids = cols.Select(c => c.Id).ToList();
        var items = await _db.CollectionItems.Where(i => ids.Contains(i.CollectionId)).ToListAsync();
        var dtos = cols.Select(c => new CollectionDto(c.Id, c.UserId, c.Name, c.CoverUrl, c.CreatedAt,
            items.Where(i => i.CollectionId == c.Id).Select(i => i.PostId).ToList())).ToList();
        return Ok(ApiResponse<List<CollectionDto>>.Ok(dtos));
    }

    [HttpPost("add-item")]
    public async Task<IActionResult> AddItem([FromQuery] int collectionId, [FromQuery] int postId)
    {
        var exists = await _db.CollectionItems.AnyAsync(i => i.CollectionId == collectionId && i.PostId == postId);
        if (!exists)
        {
            _db.CollectionItems.Add(new CollectionItem { CollectionId = collectionId, PostId = postId });
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpDelete("remove-item")]
    public async Task<IActionResult> RemoveItem([FromQuery] int collectionId, [FromQuery] int postId)
    {
        var it = await _db.CollectionItems.FirstOrDefaultAsync(i => i.CollectionId == collectionId && i.PostId == postId);
        if (it is not null) { _db.CollectionItems.Remove(it); await _db.SaveChangesAsync(); }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        var c = await _db.Collections.FindAsync(id);
        if (c is not null)
        {
            _db.CollectionItems.RemoveRange(_db.CollectionItems.Where(i => i.CollectionId == id));
            _db.Collections.Remove(c);
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
