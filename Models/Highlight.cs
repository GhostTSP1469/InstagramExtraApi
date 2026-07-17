namespace InstagramExtraApi.Models;

/// <summary>Закреплённая подборка сторис (Highlights) на профиле.</summary>
public class Highlight
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Элемент подборки (медиа сторис — имя файла основного API или URL).</summary>
public class HighlightItem
{
    public int Id { get; set; }
    public int HighlightId { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string Type { get; set; } = "image"; // image | video
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
