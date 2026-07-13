using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Звонки (аудио/видео). Такого в основном API нет.
/// Аутентификации у доп-бэкенда нет, поэтому userId передаётся явно.
/// Клиент опрашивает /Call/incoming и /Call/get для синхронизации состояния.
/// </summary>
[ApiController]
[Route("Call")]
public class CallController : ControllerBase
{
    private readonly AppDbContext _db;
    public CallController(AppDbContext db) => _db = db;

    public record StartCallDto(string CallerId, string CallerName, string CalleeId, string CalleeName, string Type);

    /// <summary>Начать звонок (создаётся в статусе ringing).</summary>
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartCallDto dto)
    {
        var call = new Call
        {
            CallerId = dto.CallerId,
            CallerName = dto.CallerName,
            CalleeId = dto.CalleeId,
            CalleeName = dto.CalleeName,
            Type = dto.Type == "audio" ? "audio" : "video",
            Status = "ringing",
        };
        _db.Calls.Add(call);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Call>.Ok(call));
    }

    /// <summary>Входящие звонки (для получателя, статус ringing).</summary>
    [HttpGet("incoming")]
    public async Task<IActionResult> Incoming([FromQuery] string userId)
    {
        var calls = await _db.Calls
            .Where(c => c.CalleeId == userId && c.Status == "ringing")
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return Ok(ApiResponse<List<Call>>.Ok(calls));
    }

    /// <summary>Один звонок по id (для опроса статуса).</summary>
    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] int callId)
    {
        var call = await _db.Calls.FindAsync(callId);
        if (call is null) return Ok(ApiResponse<Call>.Fail("Call not found", 404));
        return Ok(ApiResponse<Call>.Ok(call));
    }

    /// <summary>История звонков пользователя.</summary>
    [HttpGet("history")]
    public async Task<IActionResult> History([FromQuery] string userId)
    {
        var calls = await _db.Calls
            .Where(c => c.CallerId == userId || c.CalleeId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(50)
            .ToListAsync();
        return Ok(ApiResponse<List<Call>>.Ok(calls));
    }

    [HttpPut("accept")]
    public Task<IActionResult> Accept([FromQuery] int callId) => SetStatus(callId, "accepted");

    [HttpPut("decline")]
    public Task<IActionResult> Decline([FromQuery] int callId) => SetStatus(callId, "declined");

    [HttpPut("end")]
    public Task<IActionResult> End([FromQuery] int callId) => SetStatus(callId, "ended");

    private async Task<IActionResult> SetStatus(int callId, string status)
    {
        var call = await _db.Calls.FindAsync(callId);
        if (call is null) return Ok(ApiResponse<Call>.Fail("Call not found", 404));
        call.Status = status;
        if (status is "ended" or "declined") call.EndedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Call>.Ok(call));
    }
}
