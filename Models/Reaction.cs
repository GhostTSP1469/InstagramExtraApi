namespace InstagramExtraApi.Models;

/// <summary>Эмодзи-реакция пользователя на пост (одна на пользователя+пост).</summary>
public class Reaction
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int PostId { get; set; }
    public string Emoji { get; set; } = "❤️";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
