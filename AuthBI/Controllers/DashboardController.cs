using AuthBI.Models.ViewModels;
using AuthBI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthBI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {

        private readonly QueryService _query;

        public DashboardController(QueryService query)
            => _query = query;

        [HttpGet("dashboard")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("relatorio/faturamento")]
        public async Task<IActionResult> Faturamento()
        {
            var sql = """
            SELECT NomeFantasia, CNPJ
            FROM Filial
            """;

            var rows = await _query.ExecuteAsync("filial_01", sql);
            return PartialView("_TabelaSql", rows);
        }

    }
}
