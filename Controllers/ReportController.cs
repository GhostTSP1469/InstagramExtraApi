using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>Жалобы на пользователя/пост/сторис/комментарий.</summary>
[ApiController]
[Route("Report")]
public class ReportController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReportController(AppDbContext db) => _db = db;

    public record ReportRequest(string ReporterId, string TargetType, string TargetId, string Reason);

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] ReportRequest dto)
    {
        var report = new Report
        {
            ReporterId = dto.ReporterId,
            TargetType = string.IsNullOrWhiteSpace(dto.TargetType) ? "user" : dto.TargetType.ToLower(),
            TargetId = dto.TargetId,
            Reason = dto.Reason,
        };
        _db.Reports.Add(report);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Report>.Ok(report));
    }

    /// <summary>Мои отправленные жалобы.</summary>
    [HttpGet("mine")]
    public async Task<IActionResult> Mine([FromQuery] string reporterId)
    {
        var items = await _db.Reports.Where(r => r.ReporterId == reporterId).OrderByDescending(r => r.Id).ToListAsync();
        return Ok(ApiResponse<List<Report>>.Ok(items));
    }
}
