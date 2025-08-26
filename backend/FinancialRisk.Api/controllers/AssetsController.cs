using Microsoft.AspNetCore.Mvc;

namespace FinancialRisk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        // Temporary in-memory "mock" data
        private static readonly List<object> _assets = new()
        {
            new { Id = 1, Symbol = "AAPL", Name = "Apple Inc." },
            new { Id = 2, Symbol = "MSFT", Name = "Microsoft Corp." },
            new { Id = 3, Symbol = "GOOGL", Name = "Alphabet Inc." }
        };

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_assets);
        }
    }
}
