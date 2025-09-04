using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FinancialRisk.Api.Services;

namespace FinancialRisk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InteropController : ControllerBase
    {
        private readonly ILogger<InteropController> _logger;
        private readonly UnifiedInteropService _interopService;

        public InteropController(ILogger<InteropController> logger, UnifiedInteropService interopService)
        {
            _logger = logger;
            _interopService = interopService;
        }

        // Stub implementation - methods will be added as needed
        [HttpPost("execute-python")]
        public async Task<IActionResult> ExecutePython([FromBody] object request)
        {
            _logger.LogInformation("Executing Python script via interop");
            await Task.Delay(100); // Simulate async operation
            return Ok(new { result = "Python execution completed" });
        }

        [HttpPost("execute-cpp")]
        public async Task<IActionResult> ExecuteCpp([FromBody] object request)
        {
            _logger.LogInformation("Executing C++ code via interop");
            await Task.Delay(100); // Simulate async operation
            return Ok(new { result = "C++ execution completed" });
        }
    }
}