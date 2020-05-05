using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BShop.API.Controllers.ViewModels;
using BShop.API.Data;
using BShop.API.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BShop.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _logger = NullLogger<CategoriesController>.Instance;
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryViewModel>>> GetAll()
        {
            var response = await _context.Categories
                .Select(x => new CategoryViewModel {Id = x.Id, Name = x.Name})
                .ToListAsync();

            _logger.LogInformation("{@response}", response);

            return response;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryViewModel>> Get(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var response = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name
            };

            _logger.LogInformation("{@response}", response);

            return response;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, CategoryCreateViewModel request)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            category.Name = request.Name;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<CategoryViewModel>> Post(CategoryCreateViewModel request)
        {
            var category = new Category
            {
                Name = request.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = category.Id }, new CategoryViewModel { Id = category.Id, Name = category.Name });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}