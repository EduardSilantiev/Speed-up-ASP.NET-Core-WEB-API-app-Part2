using Microsoft.AspNetCore.Mvc;
using SpeedUpCoreAPIExample.Interfaces;
using System.Threading.Tasks;

namespace SpeedUpCoreAPIExample.Contexts
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricesController : ControllerBase
    {
        private readonly IPricesService _pricesService;

        public PricesController(IPricesService pricesService)
        {
            _pricesService = pricesService;
        }

        // GET /api/prices/1
        [HttpGet("{Id:int}")]
        public async Task<IActionResult> GetPricesAsync(int id)
        {
            return await _pricesService.GetPricesAsync(id);
        }

        // POST api/prices/prepare/5
        [HttpPost("prepare/{id}")]
        public async Task<IActionResult> PreparePricessAsync(int id)
        {
            await _pricesService.PreparePricesAsync(id);

            return Ok();
        }
    }
}