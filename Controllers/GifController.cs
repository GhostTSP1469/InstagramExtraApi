using System.Text.Json;
using InstagramExtraApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// GIF для чата — прокси к Giphy (ключ в GIPHY_API_KEY или дефолтный).
/// Клиент дёргает /Gif/search и /Gif/trending, получает готовые url гифок.
/// </summary>
[ApiController]
[Route("Gif")]
public class GifController : ControllerBase
{
    private readonly IHttpClientFactory _http;
    private readonly string _key;

    public GifController(IHttpClientFactory http, IConfiguration cfg)
    {
        _http = http;
        _key = Environment.GetEnvironmentVariable("GIPHY_API_KEY")
               ?? cfg["Giphy:ApiKey"]
               ?? "9LwAaRQlBIwUuHpDHGLiqtN0JPH8ENvu";
    }

    public record GifDto(string Id, string Url, string Preview, string Title, int Width, int Height);

    [HttpGet("search")]
    public Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 24, [FromQuery] int offset = 0)
        => Fetch($"https://api.giphy.com/v1/gifs/search?api_key={_key}&q={Uri.EscapeDataString(q ?? "")}&limit={limit}&offset={offset}&rating=pg-13");

    [HttpGet("trending")]
    public Task<IActionResult> Trending([FromQuery] int limit = 24)
        => Fetch($"https://api.giphy.com/v1/gifs/trending?api_key={_key}&limit={limit}&rating=pg-13");

    private async Task<IActionResult> Fetch(string url)
    {
        try
        {
            var client = _http.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            var json = await client.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var list = new List<GifDto>();
            foreach (var g in doc.RootElement.GetProperty("data").EnumerateArray())
            {
                var img = g.GetProperty("images");
                var full = img.GetProperty("fixed_height");
                var prev = img.TryGetProperty("fixed_height_small", out var p) ? p : full;
                list.Add(new GifDto(
                    g.GetProperty("id").GetString() ?? "",
                    full.GetProperty("url").GetString() ?? "",
                    prev.GetProperty("url").GetString() ?? "",
                    g.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
                    int.TryParse(full.GetProperty("width").GetString(), out var w) ? w : 0,
                    int.TryParse(full.GetProperty("height").GetString(), out var h) ? h : 0));
            }
            return Ok(ApiResponse<List<GifDto>>.Ok(list));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<List<GifDto>>.Fail("GIF provider error: " + ex.Message, 502));
        }
    }
}
