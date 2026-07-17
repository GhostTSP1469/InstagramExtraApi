using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Блокировки пользователей. Клиент прячет контент заблокированных и не даёт им
/// взаимодействовать. userId передаётся явно (у доп-бэка нет авторизации).
/// </summary>
[ApiController]
[Route("Block")]
public class BlockController : ControllerBase
{
    private readonly AppDbContext _db;
    public BlockController(AppDbContext db) => _db = db;

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromQuery] string userId, [FromQuery] string blockedUserId)
    {
        if (userId == blockedUserId) return Ok(ApiResponse<bool>.Fail("Нельзя заблокировать себя", 400));
        var exists = await _db.Blocks.AnyAsync(b => b.UserId == userId && b.BlockedUserId == blockedUserId);
        if (!exists)
        {
            _db.Blocks.Add(new Block { UserId = userId, BlockedUserId = blockedUserId });
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> Remove([FromQuery] string userId, [FromQuery] string blockedUserId)
    {
        var row = await _db.Blocks.FirstOrDefaultAsync(b => b.UserId == userId && b.BlockedUserId == blockedUserId);
        if (row is not null) { _db.Blocks.Remove(row); await _db.SaveChangesAsync(); }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>Кого пользователь заблокировал (список id).</summary>
    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] string userId)
    {
        var ids = await _db.Blocks.Where(b => b.UserId == userId).Select(b => b.BlockedUserId).ToListAsync();
        return Ok(ApiResponse<List<string>>.Ok(ids));
    }

    /// <summary>Есть ли блок между двумя пользователями в любую сторону.</summary>
    [HttpGet("is-blocked")]
    public async Task<IActionResult> IsBlocked([FromQuery] string userId, [FromQuery] string otherId)
    {
        var blocked = await _db.Blocks.AnyAsync(b =>
            (b.UserId == userId && b.BlockedUserId == otherId) ||
            (b.UserId == otherId && b.BlockedUserId == userId));
        return Ok(ApiResponse<bool>.Ok(blocked));
    }
}
