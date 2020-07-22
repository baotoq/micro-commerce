using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using MediatR;
using UnitOfWork;

namespace Catalog.API.Application.Orders.Commands
{
    public class ChangeOrderStatusCommand : IRequest<Unit>
    {
        [JsonIgnore]
        public long Id { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }

    public class ChangeOrderStatusCommandHandler : IRequestHandler<ChangeOrderStatusCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Order> _repository;

        public ChangeOrderStatusCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<Order>();
        }

        public async Task<Unit> Handle(ChangeOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await _repository.FindAsync(request.Id, cancellationToken);

            order.OrderStatus = request.OrderStatus;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
