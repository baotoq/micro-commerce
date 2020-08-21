using System.Threading.Tasks;
using Dapper;
using Data.UnitOfWork.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ordering.API.Data;
using Ordering.API.Data.Models;

namespace Ordering.API.ApiControllers
{
    public static class OrderSchema
    {
        public const string Table = @"""Orders""";
    }

    [ApiController]
    [Route("[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly ILogger<SeedController> _logger;
        private readonly IEfUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public SeedController(ILogger<SeedController> logger, IEfUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //_unitOfWork.Connection.Open();

            for (int i = 0; i < 100; i++)
            {
                await _context.Orders.AddAsync(new Order());
                //await _unitOfWork.Repository<Order>().Query().ToListAsync();
                var result = await _unitOfWork.Connection.QueryAsync(@$"DELETE FROM {OrderSchema.Table}");
            }

            await _context.SaveChangesAsync();
            //_unitOfWork.Connection.Close();
            return Ok();
        }
    }
}
