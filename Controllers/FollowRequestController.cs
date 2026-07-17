using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Запросы на подписку к приватным аккаунтам. userId передаётся явно.
/// (У softclub follow мгновенный, поэтому «запросы» живут здесь.)
/// </summary>
[ApiController]
[Route("FollowRequest")]
public class FollowRequestController : ControllerBase
{
    private readonly AppDbContext _db;
    public FollowRequestController(AppDbContext db) => _db = db;

    public record CreateDto(string RequesterId, string RequesterName, string? RequesterImage, string TargetId);

    /// <summary>Создать запрос (или вернуть существующий).</summary>
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateDto dto)
    {
        var existing = await _db.FollowRequests.FirstOrDefaultAsync(r => r.RequesterId == dto.RequesterId && r.TargetId == dto.TargetId);
        if (existing is null)
        {
            existing = new FollowRequest
            {
                RequesterId = dto.RequesterId,
                RequesterName = dto.RequesterName,
                RequesterImage = dto.RequesterImage,
                TargetId = dto.TargetId,
                Status = "pending",
            };
            _db.FollowRequests.Add(existing);
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<FollowRequest>.Ok(existing));
    }

    /// <summary>Входящие запросы (ожидают решения владельца).</summary>
    [HttpGet("incoming")]
    public async Task<IActionResult> Incoming([FromQuery] string userId)
    {
        var items = await _db.FollowRequests
            .Where(r => r.TargetId == userId && r.Status == "pending")
            .OrderByDescending(r => r.Id).ToListAsync();
        return Ok(ApiResponse<List<FollowRequest>>.Ok(items));
    }

    /// <summary>Статус моего запроса к пользователю: none | pending | approved.</summary>
    [HttpGet("status")]
    public async Task<IActionResult> Status([FromQuery] string requesterId, [FromQuery] string targetId)
    {
        var r = await _db.FollowRequests.FirstOrDefaultAsync(x => x.RequesterId == requesterId && x.TargetId == targetId);
        return Ok(ApiResponse<string>.Ok(r?.Status ?? "none"));
    }

    [HttpPut("approve")]
    public async Task<IActionResult> Approve([FromQuery] int id)
    {
        var r = await _db.FollowRequests.FindAsync(id);
        if (r is null) return Ok(ApiResponse<bool>.Fail("Not found", 404));
        r.Status = "approved";
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpDelete("decline")]
    public async Task<IActionResult> Decline([FromQuery] int id)
    {
        var r = await _db.FollowRequests.FindAsync(id);
        if (r is not null) { _db.FollowRequests.Remove(r); await _db.SaveChangesAsync(); }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>Отменить свой запрос.</summary>
    [HttpDelete("cancel")]
    public async Task<IActionResult> Cancel([FromQuery] string requesterId, [FromQuery] string targetId)
    {
        var r = await _db.FollowRequests.FirstOrDefaultAsync(x => x.RequesterId == requesterId && x.TargetId == targetId);
        if (r is not null) { _db.FollowRequests.Remove(r); await _db.SaveChangesAsync(); }
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
