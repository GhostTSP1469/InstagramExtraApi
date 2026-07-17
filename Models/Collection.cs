namespace InstagramExtraApi.Models;

/// <summary>Папка сохранённых постов (Saved collection).</summary>
public class Collection
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Пост внутри коллекции (postId — id поста основного API).</summary>
public class CollectionItem
{
    public int Id { get; set; }
    public int CollectionId { get; set; }
    public int PostId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
