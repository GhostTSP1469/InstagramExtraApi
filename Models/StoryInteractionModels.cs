namespace InstagramExtraApi.Models;

/// <summary>
/// Реакция на сторис ОСНОВНОГО API (storyId — id из softclub). Заменяет фейк
/// через /Reaction. Одна на пользователя+сторис; повторный вызов меняет эмодзи.
/// </summary>
public class StoryReaction
{
    public int Id { get; set; }
    public int StoryId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Ответ на сторис (доставляется автору сторис). Заменяет фейк-коммент.</summary>
public class StoryReply
{
    public int Id { get; set; }
    public int StoryId { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;  // автор сторис (кому пришёл ответ)
    public string FromUserId { get; set; } = string.Empty;   // кто ответил
    public string FromUserName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Кросс-девайс отметка «просмотрено» для сторис основного API (isViewed).</summary>
public class StorySeen
{
    public int Id { get; set; }
    public int StoryId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
