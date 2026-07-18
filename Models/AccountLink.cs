namespace InstagramExtraApi.Models;

/// <summary>
/// Привязка внешнего аккаунта (Google) к пользователю основного API (softclub).
/// Позволяет подключить Google уже после обычного входа и перепривязать почту.
/// Одна привязка на userId+provider.
/// </summary>
public class AccountLink
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;          // softclub user id
    public string? UserName { get; set; }                        // softclub username (для входа через Google)
    public string Provider { get; set; } = "google";            // google | ...
    public string ProviderAccountId { get; set; } = string.Empty; // google sub (уникальный id аккаунта)
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Picture { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
