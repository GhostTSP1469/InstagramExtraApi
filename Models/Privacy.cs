namespace InstagramExtraApi.Models;

/// <summary>Настройка приватности аккаунта (по пользователю softclub).</summary>
public class Privacy
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Запрос на подписку к приватному аккаунту.</summary>
public class FollowRequest
{
    public int Id { get; set; }
    public string RequesterId { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public string? RequesterImage { get; set; }
    public string TargetId { get; set; } = string.Empty;
    public string Status { get; set; } = "pending"; // pending | approved
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
