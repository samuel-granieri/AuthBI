using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthBI.Controllers
{
    [Route("api/dre")]
    [ApiController]
    public class DashboardApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Receita = 150000,
                Custos = 90000,
                Itens = new[]
                {
                new { Nome = "Receita Bruta", Valor = 150000 },
                new { Nome = "Custos", Valor = -90000 },
                new { Nome = "Lucro", Valor = 60000 }
            }
            });
        }
    }
}
