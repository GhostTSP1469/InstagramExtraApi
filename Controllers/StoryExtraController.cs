using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>Сторис (24 часа): загрузка фото/видео, лента по пользователям, просмотр, лайк.</summary>
[ApiController]
[Route("StoryExtra")]
public class StoryExtraController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    public StoryExtraController(AppDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    public record StoryGroupDto(string UserId, string UserName, string? UserImage, List<ExtraStory> Stories);

    /// <summary>Добавить сторис (multipart: image/video).</summary>
    [HttpPost("add")]
    public async Task<IActionResult> Add(
        [FromForm] string userId, [FromForm] string userName, [FromForm] string? userImage,
        [FromForm] string? caption, [FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0) return Ok(ApiResponse<ExtraStory>.Fail("file is required", 400));
        var uploads = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads");
        Directory.CreateDirectory(uploads);
        var name = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        await using (var fs = System.IO.File.Create(Path.Combine(uploads, name)))
            await file.CopyToAsync(fs);

        var isVideo = (file.ContentType ?? "").StartsWith("video") ||
                      Path.GetExtension(file.FileName).ToLower() is ".mp4" or ".webm" or ".mov";
        var story = new ExtraStory
        {
            UserId = userId,
            UserName = userName,
            UserImage = userImage,
            Caption = caption,
            MediaUrl = $"/uploads/{name}",
            Type = isVideo ? "video" : "image",
        };
        _db.ExtraStories.Add(story);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<ExtraStory>.Ok(story));
    }

    /// <summary>Активные сторис (не истёкшие), сгруппированные по пользователю.</summary>
    [HttpGet("feed")]
    public async Task<IActionResult> Feed()
    {
        var now = DateTime.UtcNow;
        var active = await _db.ExtraStories.Where(s => s.ExpiresAt > now).OrderBy(s => s.CreatedAt).ToListAsync();
        var groups = active.GroupBy(s => s.UserId)
            .Select(g => new StoryGroupDto(g.Key, g.First().UserName, g.First().UserImage, g.ToList()))
            .ToList();
        return Ok(ApiResponse<List<StoryGroupDto>>.Ok(groups));
    }

    /// <summary>Сторис одного пользователя.</summary>
    [HttpGet("user")]
    public async Task<IActionResult> User([FromQuery] string userId)
    {
        var now = DateTime.UtcNow;
        var items = await _db.ExtraStories.Where(s => s.UserId == userId && s.ExpiresAt > now).OrderBy(s => s.CreatedAt).ToListAsync();
        return Ok(ApiResponse<List<ExtraStory>>.Ok(items));
    }

    /// <summary>Засчитать просмотр (уникально на пользователя).</summary>
    [HttpPost("view")]
    public async Task<IActionResult> View([FromQuery] int storyId, [FromQuery] string userId, [FromQuery] string userName)
    {
        var seen = await _db.StoryEvents.AnyAsync(e => e.StoryId == storyId && e.UserId == userId && e.Kind == "view");
        if (!seen)
        {
            _db.StoryEvents.Add(new StoryEvent { StoryId = storyId, UserId = userId, UserName = userName, Kind = "view" });
            await _db.SaveChangesAsync();
        }
        return Ok(ApiResponse<bool>.Ok(true));
    }

    /// <summary>Лайк/снять лайк (переключатель).</summary>
    [HttpPost("like")]
    public async Task<IActionResult> Like([FromQuery] int storyId, [FromQuery] string userId, [FromQuery] string userName)
    {
        var like = await _db.StoryEvents.FirstOrDefaultAsync(e => e.StoryId == storyId && e.UserId == userId && e.Kind == "like");
        bool liked;
        if (like is null) { _db.StoryEvents.Add(new StoryEvent { StoryId = storyId, UserId = userId, UserName = userName, Kind = "like" }); liked = true; }
        else { _db.StoryEvents.Remove(like); liked = false; }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(liked));
    }

    /// <summary>Кто смотрел/лайкал сторис + счётчики.</summary>
    [HttpGet("viewers")]
    public async Task<IActionResult> Viewers([FromQuery] int storyId)
    {
        var events = await _db.StoryEvents.Where(e => e.StoryId == storyId).ToListAsync();
        return Ok(ApiResponse<object>.Ok(new
        {
            viewCount = events.Count(e => e.Kind == "view"),
            likeCount = events.Count(e => e.Kind == "like"),
            viewers = events.Where(e => e.Kind == "view").Select(e => new { e.UserId, e.UserName }),
            likes = events.Where(e => e.Kind == "like").Select(e => new { e.UserId, e.UserName }),
        }));
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        var s = await _db.ExtraStories.FindAsync(id);
        if (s is not null) { _db.ExtraStories.Remove(s); await _db.SaveChangesAsync(); }
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
