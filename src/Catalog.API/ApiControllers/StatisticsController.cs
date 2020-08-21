using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Data.UnitOfWork.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.ApiControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IRepository<Review> _reviewRepository;
        private readonly IRepository<Order> _orderRepository;

        public StatisticsController(IRepository<Review> reviewRepository, IRepository<Order> orderRepository)
        {
            _reviewRepository = reviewRepository;
            _orderRepository = orderRepository;
        }

        [ResponseCache(Duration = 5)]
        [HttpGet("reviews")]
        public async Task<IActionResult> Reviews()
        {
            var time = DateTime.UtcNow.AddYears(-1);

            var result = await _reviewRepository.Query()
                .Where(s => s.CreatedDate > time)
                .GroupBy(s => s.CreatedDate.Month)
                .Select(s => new object[] { CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(s.Key), s.Count() })
                .ToListAsync();

            return Ok(result);
        }

        [ResponseCache(Duration = 5)]
        [HttpGet("orders")]
        public async Task<IActionResult> Orders()
        {
            var time = DateTime.UtcNow.AddYears(-1);

            var result = await _orderRepository.Query()
                .Where(s => s.CreatedDate > time)
                .GroupBy(s => s.CreatedDate.Month)
                .Select(s => new object[] { CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(s.Key), s.Count() })
                .ToListAsync();

            return Ok(result);
        }
    }
}
