using System.Text.Json;
using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Музыка в профиле: поиск трека (прокси к бесплатному iTunes Search API —
/// отдаёт 30-сек превью) и закрепление одного трека за пользователем. Другие
/// заходят в профиль и слушают. userId передаётся явно (авторизации нет).
/// </summary>
[ApiController]
[Route("Music")]
public class MusicController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _http;
    public MusicController(AppDbContext db, IHttpClientFactory http)
    {
        _db = db;
        _http = http;
    }

    public record TrackDto(string TrackName, string ArtistName, string PreviewUrl, string ArtworkUrl);
    public record SetRequest(string UserId, string TrackName, string ArtistName, string PreviewUrl, string ArtworkUrl);

    /// <summary>Поиск трека в iTunes (только те, у кого есть превью-аудио).</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 15)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(ApiResponse<List<TrackDto>>.Ok(new()));
        try
        {
            var client = _http.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            var url = $"https://itunes.apple.com/search?term={Uri.EscapeDataString(q)}&media=music&entity=song&limit={limit}";
            var json = await client.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var list = new List<TrackDto>();
            foreach (var r in doc.RootElement.GetProperty("results").EnumerateArray())
            {
                var preview = r.TryGetProperty("previewUrl", out var pv) ? pv.GetString() ?? "" : "";
                if (string.IsNullOrEmpty(preview)) continue; // без превью закреплять нечего
                list.Add(new TrackDto(
                    r.TryGetProperty("trackName", out var tn) ? tn.GetString() ?? "" : "",
                    r.TryGetProperty("artistName", out var an) ? an.GetString() ?? "" : "",
                    preview,
                    r.TryGetProperty("artworkUrl100", out var aw) ? aw.GetString() ?? "" : ""));
            }
            return Ok(ApiResponse<List<TrackDto>>.Ok(list));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<List<TrackDto>>.Fail("Music provider error: " + ex.Message, 502));
        }
    }

    /// <summary>Закрепить трек в профиле (один на пользователя).</summary>
    [HttpPost("set")]
    public async Task<IActionResult> Set([FromBody] SetRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.PreviewUrl))
            return Ok(ApiResponse<ProfileMusic>.Fail("userId and previewUrl required", 400));

        var row = await _db.ProfileMusics.FirstOrDefaultAsync(m => m.UserId == dto.UserId);
        if (row is null)
        {
            row = new ProfileMusic { UserId = dto.UserId };
            _db.ProfileMusics.Add(row);
        }
        row.TrackName = dto.TrackName;
        row.ArtistName = dto.ArtistName;
        row.PreviewUrl = dto.PreviewUrl;
        row.ArtworkUrl = dto.ArtworkUrl;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<ProfileMusic>.Ok(row));
    }

    /// <summary>Закреплённый трек пользователя (или null).</summary>
    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] string userId)
    {
        var row = await _db.ProfileMusics.FirstOrDefaultAsync(m => m.UserId == userId);
        return Ok(ApiResponse<ProfileMusic?>.Ok(row));
    }

    /// <summary>Снять музыку с профиля.</summary>
    [HttpDelete("remove")]
    public async Task<IActionResult> Remove([FromQuery] string userId)
    {
        var row = await _db.ProfileMusics.FirstOrDefaultAsync(m => m.UserId == userId);
        if (row is not null)
        {
            _db.ProfileMusics.Remove(row);
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
