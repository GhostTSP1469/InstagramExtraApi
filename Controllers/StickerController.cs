using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>Каталог стикеров для чата.</summary>
[ApiController]
[Route("Sticker")]
public class StickerController : ControllerBase
{
    private readonly AppDbContext _db;
    public StickerController(AppDbContext db) => _db = db;

    /// <summary>Список паков (уникальные названия).</summary>
    [HttpGet("packs")]
    public async Task<IActionResult> Packs()
    {
        var packs = await _db.Stickers.Select(s => s.Pack).Distinct().ToListAsync();
        return Ok(ApiResponse<List<string>>.Ok(packs));
    }

    /// <summary>Стикеры пака (или все, если pack не задан).</summary>
    [HttpGet("get")]
    public async Task<IActionResult> Get([FromQuery] string? pack)
    {
        var q = _db.Stickers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(pack)) q = q.Where(s => s.Pack == pack);
        return Ok(ApiResponse<List<Sticker>>.Ok(await q.ToListAsync()));
    }

    /// <summary>Добавить свой стикер в каталог.</summary>
    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] Sticker sticker)
    {
        sticker.Id = 0;
        _db.Stickers.Add(sticker);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Sticker>.Ok(sticker));
    }
}
