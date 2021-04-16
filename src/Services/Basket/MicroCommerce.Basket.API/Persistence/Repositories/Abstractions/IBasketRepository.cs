using System.Threading.Tasks;
using MicroCommerce.Basket.API.Models;

namespace MicroCommerce.Basket.API.Persistence.Repositories.Abstractions
{
    public interface IBasketRepository
    {
        Task<CustomerBasket> GetBasketAsync(string buyerId);
        Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket);
        Task DeleteBasketAsync(string id);
    }
}
