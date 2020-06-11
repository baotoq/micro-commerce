using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Catalog.API.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnitOfWork;

namespace Catalog.API.BackgroundServices
{
    public class ApproveReplyBackgroundService : BaseBackgroundService
    {

        public ApproveReplyBackgroundService(IServiceProvider services) : base(services)
        {
        }

        public override string BackgroundName { get; } = "Approve replies job";
        public override TimeSpan DelayTime { get; } = TimeSpan.FromMinutes(1);

        public override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            using var scope = ServiceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApproveReplyBackgroundService>>();

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var utcNow = DateTime.UtcNow;

            var replies = await unitOfWork.Repository<Reply>()
                .Query()
                .Where(s => s.ReplyStatus == ReplyStatus.Pending && s.CreatedDate.AddMinutes(5) <= utcNow)
                .ToListAsync(cancellationToken);

            foreach (var review in replies)
            {
                review.ReplyStatus = ReplyStatus.Approved;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Approved {count} replies with Id: {replies}", replies.Count, replies.Select(s => s.Id));
        }
    }
}
