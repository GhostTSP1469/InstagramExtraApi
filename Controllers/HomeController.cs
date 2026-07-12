using Microsoft.AspNetCore.Mvc;

namespace InstagramExtraApi.Controllers;

[ApiController]
[Route("/")]
public class HomeController : ControllerBase
{
    /// <summary>Health-check + подсказка, где Swagger.</summary>
    [HttpGet]
    public IActionResult Index() => Ok(new
    {
        service = "InstagramExtraApi",
        status = "ok",
        swagger = "/swagger",
        note = "Дополнительный бэкенд: ручки, которых нет/которые сломаны в основном API.",
    });
}
