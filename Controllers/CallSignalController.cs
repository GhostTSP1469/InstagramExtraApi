using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// WebRTC-сигналинг для реального видеозвонка P2P. Клиенты обмениваются
/// offer/answer/ICE-кандидатами через эти ручки (опрос get-signals по sinceId).
/// Дополняет /Call (состояние звонка) до полноценного звонка.
/// </summary>
[ApiController]
[Route("Call")]
public class CallSignalController : ControllerBase
{
    private readonly AppDbContext _db;
    public CallSignalController(AppDbContext db) => _db = db;

    public record SignalDto(int CallId, string FromUserId, string Kind, string Payload);

    /// <summary>Отправить сигнал (offer|answer|candidate) второй стороне.</summary>
    [HttpPost("send-signal")]
    public async Task<IActionResult> Send([FromBody] SignalDto dto)
    {
        var sig = new CallSignal
        {
            CallId = dto.CallId,
            FromUserId = dto.FromUserId,
            Kind = dto.Kind,
            Payload = dto.Payload,
        };
        _db.CallSignals.Add(sig);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<CallSignal>.Ok(sig));
    }

    /// <summary>
    /// Получить сигналы звонка после sinceId (для опроса). Отдаёт чужие сигналы
    /// (не свои), чтобы каждая сторона забирала только адресованное ей.
    /// </summary>
    [HttpGet("get-signals")]
    public async Task<IActionResult> Get([FromQuery] int callId, [FromQuery] string userId, [FromQuery] int sinceId = 0)
    {
        var signals = await _db.CallSignals
            .Where(s => s.CallId == callId && s.Id > sinceId && s.FromUserId != userId)
            .OrderBy(s => s.Id)
            .ToListAsync();
        return Ok(ApiResponse<List<CallSignal>>.Ok(signals));
    }
}
