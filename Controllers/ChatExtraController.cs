using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Расширенный чат: то, чего нет в основном API — гифки, стикеры, голосовые,
/// видео/файлы, поиск по сообщениям и фильтр по типу медиа.
/// </summary>
[ApiController]
[Route("ChatExtra")]
public class ChatExtraController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    public ChatExtraController(AppDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    public record SendDto(int ChatId, string SenderId, string SenderName, string Type, string? Text, string? MediaUrl, string? FileName);

    /// <summary>Отправить текст/gif/стикер (без файла, JSON).</summary>
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendDto dto)
    {
        var msg = new ExtraMessage
        {
            ChatId = dto.ChatId,
            SenderId = dto.SenderId,
            SenderName = dto.SenderName,
            Type = Normalize(dto.Type),
            Text = dto.Text,
            MediaUrl = dto.MediaUrl,
            FileName = dto.FileName,
        };
        _db.ExtraMessages.Add(msg);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<ExtraMessage>.Ok(msg));
    }

    /// <summary>Отправить файл (image/video/file/voice) — multipart.</summary>
    [HttpPost("send-file")]
    public async Task<IActionResult> SendFile(
        [FromForm] int chatId, [FromForm] string senderId, [FromForm] string senderName,
        [FromForm] string type, [FromForm] IFormFile file, [FromForm] int? durationSec, [FromForm] string? text)
    {
        if (file is null || file.Length == 0) return Ok(ApiResponse<ExtraMessage>.Fail("file is required", 400));

        var uploads = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads");
        Directory.CreateDirectory(uploads);
        var name = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        await using (var fs = System.IO.File.Create(Path.Combine(uploads, name)))
            await file.CopyToAsync(fs);

        var msg = new ExtraMessage
        {
            ChatId = chatId,
            SenderId = senderId,
            SenderName = senderName,
            Type = Normalize(type),
            Text = text,
            MediaUrl = $"/uploads/{name}",
            FileName = file.FileName,
            DurationSec = durationSec,
        };
        _db.ExtraMessages.Add(msg);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<ExtraMessage>.Ok(msg));
    }

    /// <summary>Сообщения чата с фильтром по типу медиа и поиском по тексту/имени файла.</summary>
    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] int chatId, [FromQuery] string? type, [FromQuery] string? q)
    {
        var query = _db.ExtraMessages.Where(m => m.ChatId == chatId);

        // Фильтр по типу: video | file | voice | image | gif | sticker | media(всё кроме текста)
        if (!string.IsNullOrWhiteSpace(type))
        {
            type = type.ToLower();
            query = type == "media"
                ? query.Where(m => m.Type != "text")
                : query.Where(m => m.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.ToLower();
            query = query.Where(m => (m.Text != null && m.Text.ToLower().Contains(s))
                                  || (m.FileName != null && m.FileName.ToLower().Contains(s)));
        }

        var items = await query.OrderBy(m => m.Id).ToListAsync();
        return Ok(ApiResponse<List<ExtraMessage>>.Ok(items));
    }

    /// <summary>Глобальный поиск по всем сообщениям пользователя (по всем чатам).</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string userId, [FromQuery] string q, [FromQuery] string? type)
    {
        var s = (q ?? "").ToLower();
        var query = _db.ExtraMessages.Where(m => m.SenderId == userId);
        if (!string.IsNullOrWhiteSpace(type))
        {
            var t = type.ToLower();
            query = t == "media" ? query.Where(m => m.Type != "text") : query.Where(m => m.Type == t);
        }
        if (!string.IsNullOrWhiteSpace(s))
            query = query.Where(m => (m.Text != null && m.Text.ToLower().Contains(s))
                                  || (m.FileName != null && m.FileName.ToLower().Contains(s)));
        var items = await query.OrderByDescending(m => m.Id).Take(100).ToListAsync();
        return Ok(ApiResponse<List<ExtraMessage>>.Ok(items));
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        var m = await _db.ExtraMessages.FindAsync(id);
        if (m is not null) { _db.ExtraMessages.Remove(m); await _db.SaveChangesAsync(); }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    private static string Normalize(string? type)
    {
        type = (type ?? "text").ToLower();
        return type is "text" or "image" or "video" or "file" or "voice" or "gif" or "sticker" ? type : "text";
    }
}
