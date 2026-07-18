using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Привязка внешних аккаунтов (Google) к пользователю softclub — подключение
/// из настроек уже после обычного входа + перепривязка почты. userId (softclub)
/// передаётся явно (авторизации у доп-бэка нет).
/// </summary>
[ApiController]
[Route("AccountLink")]
public class AccountLinkController : ControllerBase
{
    private readonly AppDbContext _db;
    public AccountLinkController(AppDbContext db) => _db = db;

    public record LinkRequest(string UserId, string? UserName, string? Provider, string ProviderAccountId, string Email, string? Name, string? Picture);
    public record RebindEmailRequest(string UserId, string? Provider, string Email);

    private static string Norm(string? provider) => string.IsNullOrWhiteSpace(provider) ? "google" : provider.ToLower();

    /// <summary>Привязать/обновить внешний аккаунт к пользователю.</summary>
    [HttpPost("link")]
    public async Task<IActionResult> Link([FromBody] LinkRequest dto)
    {
        var provider = Norm(dto.Provider);

        // Нельзя привязать один и тот же Google-аккаунт к разным softclub-юзерам.
        var takenByOther = await _db.AccountLinks.FirstOrDefaultAsync(
            a => a.Provider == provider && a.ProviderAccountId == dto.ProviderAccountId && a.UserId != dto.UserId);
        if (takenByOther is not null)
            return Ok(ApiResponse<AccountLink>.Fail("Этот Google-аккаунт уже привязан к другому пользователю", 409));

        var link = await _db.AccountLinks.FirstOrDefaultAsync(a => a.UserId == dto.UserId && a.Provider == provider);
        if (link is null)
        {
            link = new AccountLink { UserId = dto.UserId, Provider = provider };
            _db.AccountLinks.Add(link);
        }
        link.ProviderAccountId = dto.ProviderAccountId;
        link.Email = dto.Email;
        link.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.UserName)) link.UserName = dto.UserName;
        link.Picture = dto.Picture;
        link.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<AccountLink>.Ok(link));
    }

    /// <summary>Привязки пользователя.</summary>
    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] string userId)
    {
        var links = await _db.AccountLinks.Where(a => a.UserId == userId).ToListAsync();
        return Ok(ApiResponse<List<AccountLink>>.Ok(links));
    }

    /// <summary>К какому softclub-пользователю привязан данный внешний аккаунт (для входа через Google).</summary>
    [HttpGet("by-provider-account")]
    public async Task<IActionResult> ByProviderAccount([FromQuery] string providerAccountId, [FromQuery] string? provider)
    {
        var p = Norm(provider);
        var link = await _db.AccountLinks.FirstOrDefaultAsync(a => a.Provider == p && a.ProviderAccountId == providerAccountId);
        return Ok(ApiResponse<AccountLink?>.Ok(link));
    }

    /// <summary>Привязка по email — чтобы вход через Google находил аккаунт на ЛЮБОМ
    /// устройстве (не завися от локального хранилища браузера).</summary>
    [HttpGet("by-email")]
    public async Task<IActionResult> ByEmail([FromQuery] string email, [FromQuery] string? provider)
    {
        var p = Norm(provider);
        var e = (email ?? "").Trim().ToLower();
        var link = await _db.AccountLinks.FirstOrDefaultAsync(a => a.Provider == p && a.Email.ToLower() == e);
        return Ok(ApiResponse<AccountLink?>.Ok(link));
    }

    /// <summary>Перепривязать почту существующей привязки.</summary>
    [HttpPut("rebind-email")]
    public async Task<IActionResult> RebindEmail([FromBody] RebindEmailRequest dto)
    {
        var provider = Norm(dto.Provider);
        var link = await _db.AccountLinks.FirstOrDefaultAsync(a => a.UserId == dto.UserId && a.Provider == provider);
        if (link is null) return Ok(ApiResponse<AccountLink>.Fail("Привязка не найдена", 404));
        link.Email = dto.Email;
        link.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<AccountLink>.Ok(link));
    }

    /// <summary>Отвязать внешний аккаунт.</summary>
    [HttpDelete("unlink")]
    public async Task<IActionResult> Unlink([FromQuery] string userId, [FromQuery] string? provider)
    {
        var p = Norm(provider);
        var link = await _db.AccountLinks.FirstOrDefaultAsync(a => a.UserId == userId && a.Provider == p);
        if (link is not null) { _db.AccountLinks.Remove(link); await _db.SaveChangesAsync(); }
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
