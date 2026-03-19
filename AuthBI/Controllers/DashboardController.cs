using AuthBI.Models.ViewModels;
using AuthBI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthBI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
    
        private static int _consulta = 0;

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ConsultaBanco()
        {


            _consulta++;
            return PartialView("_TabelaSql", _consulta);
        }

    }
}
