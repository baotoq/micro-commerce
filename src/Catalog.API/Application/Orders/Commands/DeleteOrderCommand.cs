using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Data.UnitOfWork.EF;
using MediatR;

namespace Catalog.API.Application.Orders.Commands
{
    public class DeleteOrderCommand : IRequest<Unit>
    {
        public long Id { get; set; }

        public DeleteOrderCommand(long id)
        {
            Id = id;
        }
    }

    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Unit>
    {
        private readonly IEfUnitOfWork _unitOfWork;
        private readonly IRepository<Order> _repository;

        public DeleteOrderCommandHandler(IEfUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<Order>();
        }

        public async Task<Unit> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _repository.FindAsync(request.Id, cancellationToken);

            _repository.Remove(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
