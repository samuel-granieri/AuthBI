using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthBI.Controllers
{
    public class DashboardController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
