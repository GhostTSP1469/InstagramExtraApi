namespace InstagramExtraApi.Models;

/// <summary>Блокировка пользователя (UserId заблокировал BlockedUserId).</summary>
public class Block
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string BlockedUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Жалоба на пользователя/пост/сторис/комментарий.</summary>
public class Report
{
    public int Id { get; set; }
    public string ReporterId { get; set; } = string.Empty;
    public string TargetType { get; set; } = "user"; // user | post | story | comment
    public string TargetId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
