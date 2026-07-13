namespace InstagramExtraApi.Models;

/// <summary>Звонок (аудио/видео). Хранит состояние и участников.</summary>
public class Call
{
    public int Id { get; set; }
    public string CallerId { get; set; } = string.Empty;
    public string CallerName { get; set; } = string.Empty;
    public string CalleeId { get; set; } = string.Empty;
    public string CalleeName { get; set; } = string.Empty;
    public string Type { get; set; } = "video"; // video | audio
    public string Status { get; set; } = "ringing"; // ringing | accepted | declined | ended | missed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
}
