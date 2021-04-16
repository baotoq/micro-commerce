using System.Threading.Tasks;
using MicroCommerce.Basket.API.Models;
using MicroCommerce.Basket.API.Persistence.Repositories.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.Basket.API.Controllers.Http
{
    [Authorize]
    [ApiController]
    [Route("api/basket")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _repository;

        public BasketController(IBasketRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerBasket>> GetBasketByIdAsync(string id)
        {
            var basket = await _repository.GetBasketAsync(id);

            return Ok(basket ?? new CustomerBasket(id));
        }

        [HttpPut]
        public async Task<ActionResult<CustomerBasket>> UpdateBasketAsync([FromBody] CustomerBasket value)
        {
            return Ok(await _repository.UpdateBasketAsync(value));
        }

        [HttpDelete("{id}")]
        public async Task DeleteBasketByIdAsync(string id)
        {
            await _repository.DeleteBasketAsync(id);
        }
    }
}
