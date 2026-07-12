using System.ComponentModel.DataAnnotations;

namespace InstagramExtraApi.Dtos;

public class AddLocationDto
{
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string State { get; set; } = string.Empty;
    [Required] public string ZipCode { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
}

/// <summary>
/// То же тело, что шлёт фронт в основной бэкенд. Там update-Location падает
/// с 400 (не настроен маппинг UpdateLocationDto -> Location) — здесь работает.
/// </summary>
public class UpdateLocationDto
{
    [Required] public int LocationId { get; set; }
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string State { get; set; } = string.Empty;
    [Required] public string ZipCode { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
}
