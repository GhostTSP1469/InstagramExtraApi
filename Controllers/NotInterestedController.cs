using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Система «Не интересует»: пользователь помечает пост, клиент прячет такие
/// посты из ленты. userId передаётся явно (авторизации у доп-бэка нет).
/// </summary>
[ApiController]
[Route("NotInterested")]
public class NotInterestedController : ControllerBase
{
    private readonly AppDbContext _db;
    public NotInterestedController(AppDbContext db) => _db = db;

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromQuery] string userId, [FromQuery] int postId)
    {
        var exists = await _db.NotInterested.AnyAsync(x => x.UserId == userId && x.PostId == postId);
        if (!exists)
        {
            _db.NotInterested.Add(new NotInterested { UserId = userId, PostId = postId });
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> Remove([FromQuery] string userId, [FromQuery] int postId)
    {
        var row = await _db.NotInterested.FirstOrDefaultAsync(x => x.UserId == userId && x.PostId == postId);
        if (row is not null)
        {
            _db.NotInterested.Remove(row);
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>Список postId, которые пользователь скрыл.</summary>
    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] string userId)
    {
        var ids = await _db.NotInterested.Where(x => x.UserId == userId).Select(x => x.PostId).ToListAsync();
        return Ok(ApiResponse<List<int>>.Ok(ids));
    }
}
