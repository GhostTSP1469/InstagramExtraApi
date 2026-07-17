using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>Ответы на комментарии (треды). userId передаётся явно.</summary>
[ApiController]
[Route("CommentReply")]
public class CommentReplyController : ControllerBase
{
    private readonly AppDbContext _db;
    public CommentReplyController(AppDbContext db) => _db = db;

    public record AddDto(int PostId, int PostCommentId, string UserId, string UserName, string? UserImage, string Text);

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] AddDto dto)
    {
        var reply = new CommentReply
        {
            PostId = dto.PostId,
            PostCommentId = dto.PostCommentId,
            UserId = dto.UserId,
            UserName = dto.UserName,
            UserImage = dto.UserImage,
            Text = dto.Text,
        };
        _db.CommentReplies.Add(reply);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<CommentReply>.Ok(reply));
    }

    /// <summary>Все ответы поста (клиент группирует по postCommentId).</summary>
    [HttpGet("by-post")]
    public async Task<IActionResult> ByPost([FromQuery] int postId)
    {
        var items = await _db.CommentReplies.Where(r => r.PostId == postId).OrderBy(r => r.Id).ToListAsync();
        return Ok(ApiResponse<List<CommentReply>>.Ok(items));
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> Remove([FromQuery] int id)
    {
        var r = await _db.CommentReplies.FindAsync(id);
        if (r is not null) { _db.CommentReplies.Remove(r); await _db.SaveChangesAsync(); }
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
