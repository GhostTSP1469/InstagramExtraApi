using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>Приватность аккаунта (публичный/закрытый). userId передаётся явно.</summary>
[ApiController]
[Route("Privacy")]
public class PrivacyController : ControllerBase
{
    private readonly AppDbContext _db;
    public PrivacyController(AppDbContext db) => _db = db;

    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] string userId)
    {
        var p = await _db.Privacies.FirstOrDefaultAsync(x => x.UserId == userId);
        return Ok(ApiResponse<object>.Ok(new { userId, isPrivate = p?.IsPrivate ?? false }));
    }

    [HttpPut("set")]
    public async Task<IActionResult> Set([FromQuery] string userId, [FromQuery] bool isPrivate)
    {
        var p = await _db.Privacies.FirstOrDefaultAsync(x => x.UserId == userId);
        if (p is null) { p = new Privacy { UserId = userId }; _db.Privacies.Add(p); }
        p.IsPrivate = isPrivate;
        p.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { userId, isPrivate }));
    }
}
