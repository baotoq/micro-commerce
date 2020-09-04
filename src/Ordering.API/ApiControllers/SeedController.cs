using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Data.UnitOfWork.Dapper.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ordering.API.Data.Models;

namespace Ordering.API.ApiControllers
{
    public static class OrderSchema
    {
        public const string Table = "orders";
    }

    [ApiController]
    [Route("[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly ILogger<SeedController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public SeedController(ILogger<SeedController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //_unitOfWork.Connection.Open();
            using (var scope = new TransactionScope())
            {
                var a2aaa = await _unitOfWork.Connection.QueryAsync<Order>($@"SELECT * FROM {OrderSchema.Table}");
                await _unitOfWork.Repository<Order>().AddAsync(new Order());
                var a2aa2a = await _unitOfWork.Connection.QueryAsync<Order>($@"SELECT * FROM {OrderSchema.Table}");
                var a2a = await _unitOfWork.Repository<Order>().FindAsync(1);

            }

            await _unitOfWork.Repository<Order>().AddAsync(new Order());
            var aaaa = await _unitOfWork.Connection.QueryAsync<Order>($@"SELECT * FROM {OrderSchema.Table}");
            var aa = await _unitOfWork.Repository<Order>().FindAsync(1);
           // _unitOfWork.Commit();
            //_unitOfWork.Connection.Close();
            return Ok();
        }
    }
}
