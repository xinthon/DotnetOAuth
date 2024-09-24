using Microsoft.AspNetCore.Mvc;

namespace Dotnet0Auth.ApiService.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
