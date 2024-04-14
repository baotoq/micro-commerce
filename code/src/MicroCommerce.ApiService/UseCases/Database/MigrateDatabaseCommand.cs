using MediatR;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.UseCases.Database;

public record MigrateDatabaseCommand : IRequest<MigrateDataResponse>
{
    public static Func<IMediator, Task<MigrateDataResponse>> EndpointHandler => mediator => mediator.Send(new MigrateDatabaseCommand());
}

public class MigrateDatabaseCommandHandler(ApplicationDbContext context) : IRequestHandler<MigrateDatabaseCommand, MigrateDataResponse>
{
    public async Task<MigrateDataResponse> Handle(MigrateDatabaseCommand request, CancellationToken cancellationToken)
    {
        await context.Database.EnsureCreatedAsync(cancellationToken);
            
        return new MigrateDataResponse();
    }
}

public record MigrateDataResponse();