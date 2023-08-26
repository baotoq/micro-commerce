using Domain;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Controllers;

[ApiController]
[Route("[controller]")]
public class CartsController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet(Name = "Get Carts")]
    public IActionResult GetAll()
    {
        var result = _context.Carts.Where(s => s.Id == "");
        return Ok(result);
    }
}