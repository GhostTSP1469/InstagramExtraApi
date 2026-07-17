namespace InstagramExtraApi.Models;

/// <summary>
/// Ответ на комментарий (тред). Комменты основного API плоские, поэтому ответы
/// храним здесь, привязывая к postCommentId (+ postId для батч-выборки).
/// </summary>
public class CommentReply
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int PostCommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserImage { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
