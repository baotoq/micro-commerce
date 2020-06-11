using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Catalog.API.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UnitOfWork;

namespace Catalog.API.BackgroundServices
{
    public class ApproveReviewBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ApproveReviewBackgroundService> _logger;

        public ApproveReviewBackgroundService(IServiceProvider services, ILogger<ApproveReviewBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{service} is running", nameof(ApproveReviewBackgroundService));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("{service} is doing background work", nameof(ApproveReviewBackgroundService));

                    await ProcessAsync(stoppingToken);

                    _logger.LogInformation("{service} have done background work", nameof(ApproveReviewBackgroundService));

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing {service}", nameof(ApproveReviewBackgroundService));
                }
            }
        }

        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var utcNow = DateTime.UtcNow;

            var reviews = await unitOfWork.Repository<Review>()
                .Query()
                .Where(s => s.ReviewStatus == ReviewStatus.Pending && s.CreatedDate.AddMinutes(5) <= utcNow)
                .ToListAsync(cancellationToken);

            foreach (var review in reviews)
            {
                review.ReviewStatus = ReviewStatus.Approved;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Approved {count} reviews with Id: {reviews}", reviews.Count, reviews.Select(s => s.Id));
        }
    }
}
