using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;
using Tutorial9.Services;

namespace Tutorial9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IDbService dbService;

        public WarehouseController(IDbService dbService)
        {
            this.dbService = dbService;
        }

        [HttpPost]
        public async Task<IActionResult> FulfillOrder([FromBody] FulfillOrderDTO order)
        {
            try
            {
                return Ok(await dbService.FulfillOrder(order));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
        
        [HttpPost("procedure")]
        public async Task<IActionResult> FulfillOrderProcedure([FromBody] FulfillOrderDTO order)
        {
            try
            {
                return Ok(await dbService.FulfillOrderProcedure(order));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
        
    }
}
