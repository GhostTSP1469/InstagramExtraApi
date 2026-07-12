using InstagramExtraApi.Common;
using InstagramExtraApi.Data;
using InstagramExtraApi.Dtos;
using InstagramExtraApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Controllers;

/// <summary>
/// Локации. Роуты и формы ответов совпадают с основным бэкендом, но здесь
/// update-Location реально работает (в основном API он падает с 400 —
/// не настроен AutoMapper UpdateLocationDto -> Location).
/// </summary>
[ApiController]
[Route("Location")]
public class LocationController : ControllerBase
{
    private readonly AppDbContext _db;
    public LocationController(AppDbContext db) => _db = db;

    [HttpGet("get-Locations")]
    public async Task<IActionResult> GetLocations(
        [FromQuery] string? city, [FromQuery] string? state,
        [FromQuery] string? zipCode, [FromQuery] string? country,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _db.Locations.AsQueryable();
        if (!string.IsNullOrWhiteSpace(city)) query = query.Where(l => l.City.ToLower().Contains(city.ToLower()));
        if (!string.IsNullOrWhiteSpace(state)) query = query.Where(l => l.State.ToLower().Contains(state.ToLower()));
        if (!string.IsNullOrWhiteSpace(zipCode)) query = query.Where(l => l.ZipCode.Contains(zipCode));
        if (!string.IsNullOrWhiteSpace(country)) query = query.Where(l => l.Country.ToLower().Contains(country.ToLower()));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(l => l.LocationId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResponse<Location>(items, pageNumber, pageSize, total));
    }

    [HttpGet("get-Location-by-id")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var loc = await _db.Locations.FindAsync(id);
        if (loc is null) return Ok(ApiResponse<Location>.Fail("Location not found", 404));
        return Ok(ApiResponse<Location>.Ok(loc));
    }

    [HttpPost("add-Location")]
    public async Task<IActionResult> Add([FromBody] AddLocationDto dto)
    {
        var loc = new Location
        {
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            Country = dto.Country,
        };
        _db.Locations.Add(loc);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Location>.Ok(loc));
    }

    /// <summary>Рабочее обновление локации (в основном API падало с 400).</summary>
    [HttpPut("update-Location")]
    public async Task<IActionResult> Update([FromBody] UpdateLocationDto dto)
    {
        var loc = await _db.Locations.FindAsync(dto.LocationId);
        if (loc is null) return Ok(ApiResponse<Location>.Fail("Location not found", 404));

        loc.City = dto.City;
        loc.State = dto.State;
        loc.ZipCode = dto.ZipCode;
        loc.Country = dto.Country;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<Location>.Ok(loc));
    }

    [HttpDelete("delete-Location")]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        var loc = await _db.Locations.FindAsync(id);
        if (loc is null) return Ok(ApiResponse<bool>.Fail("Location not found", 404));

        _db.Locations.Remove(loc);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
