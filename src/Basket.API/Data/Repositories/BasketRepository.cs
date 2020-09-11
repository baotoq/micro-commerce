using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Basket.API.Data.Repositories
{
    public interface IBasketRepository
    {
    }

    public class BasketRepository : IBasketRepository
    {
        private readonly ILogger<BasketRepository> _logger;
        private readonly IDatabase _database;

        public BasketRepository(ILogger<BasketRepository> logger, IConnectionMultiplexer connectionMultiplexer)
        {
            _logger = logger;
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task<Models.Basket> FindAsync(string id)
        {
            var data = await _database.StringGetAsync(id);

            return data.IsNullOrEmpty ? null : JsonConvert.DeserializeObject<Models.Basket>(data);
        }

        public async Task<Models.Basket> UpdateAsync(Models.Basket basket)
        {
            var created = await _database.StringSetAsync(basket.CustomerId, JsonConvert.SerializeObject(basket));

            if (!created)
            {
                _logger.LogInformation("Problem occur persisting the item.");
                throw new InvalidOperationException();
            }

            _logger.LogInformation("Basket item persisted successfully.");

            return await FindAsync(basket.CustomerId);
        }
    }
}
