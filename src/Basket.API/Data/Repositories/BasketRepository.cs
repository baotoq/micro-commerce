using System;
using System.Threading;
using System.Threading.Tasks;
using Data.UnitOfWork.Dapper.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Basket.API.Data.Repositories
{
    public class BasketRepository : IRepository<Models.Basket, long>
    {
        private readonly ILogger<BasketRepository> _logger;
        private readonly IDatabase _database;

        public BasketRepository(ILogger<BasketRepository> logger, IConnectionMultiplexer connectionMultiplexer)
        {
            _logger = logger;
            _database = connectionMultiplexer.GetDatabase();
        }

        public async ValueTask<Models.Basket> FindAsync(long id, CancellationToken cancellationToken = default)
        {
            var data = await _database.StringGetAsync(id.ToString());

            return data.IsNullOrEmpty ? null : JsonConvert.DeserializeObject<Models.Basket>(data);
        }

        public Task AddAsync(Models.Basket entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Models.Basket> UpdateAsync(Models.Basket basket)
        {
            var created = await _database.StringSetAsync(basket.CustomerId.ToString(), JsonConvert.SerializeObject(basket));

            if (!created)
            {
                _logger.LogInformation("Problem occur persisting the item.");
                throw new InvalidOperationException();
            }

            _logger.LogInformation("Basket item persisted successfully.");

            return await FindAsync(basket.CustomerId);
        }

        public Task RemoveAsync(Models.Basket entity, CancellationToken cancellationToken = default) => _database.KeyDeleteAsync(entity.CustomerId.ToString());
    }
}
