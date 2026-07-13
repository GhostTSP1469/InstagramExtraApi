namespace InstagramExtraApi.Models;

/// <summary>Репост — пользователь делится чужим постом (один репост на пользователя+пост).</summary>
public class Repost
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int PostId { get; set; }
    public string OriginalAuthorId { get; set; } = string.Empty;
    public string OriginalAuthorName { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty; // подпись, которую добавил репостящий
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
