using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Catalog.API.ApiControllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IRepository<Review> _repository;

        public StatisticsController(IRepository<Review> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var time = DateTime.UtcNow.AddYears(-1);

            var result = await _repository.Query()
                .Where(s => s.CreatedDate > time)
                .GroupBy(s => s.CreatedDate.Month)
                .Select(s => new object[] { s.Key, s.Count() })
                .ToListAsync();

            return Ok(result);
        }
    }
}
