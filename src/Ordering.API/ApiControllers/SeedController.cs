using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Data.UnitOfWork;
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

            for (int i = 0; i < 2; i++)
            {
                await _unitOfWork.Repository<Order>().AddAsync(new Order());
                //await _unitOfWork.Repository<Order>().Query().ToListAsync();
                //await _unitOfWork.Connection.QueryAsync($@"INSERT INTO {OrderSchema.Table} (""SubTotal"", ""OrderStatus"", ""CreatedDate"", ""LastModified"") VALUES (@SubTotal, @OrderStatus, @CreatedDate, @LastModified)", new Order());
                var aaaa = await _unitOfWork.Connection.QueryAsync<List<Order>>($@"SELECT * FROM {OrderSchema.Table}");
                var aa = await _unitOfWork.Repository<Order>().FindAsync(1);
            }
            _unitOfWork.Commit();
            //_unitOfWork.Connection.Close();
            return Ok();
        }
    }
}
