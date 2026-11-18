using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SlidesViewer.Web.Controllers;

[Route("film")]
public class FilmAnalysisController : Controller
{
    private readonly IConfiguration _configuration;

    public FilmAnalysisController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.HostApiBaseUrl = _configuration["SlidesViewerApi:BaseUrl"] ?? "http://localhost:5041/";
        return View();
    }
}
