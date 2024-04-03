using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.UseCases.Categories;

public record GetCategoriesQuery : IRequest<IEnumerable<CategoryViewModel>>
{
    public static Func<IMediator, Task<IEnumerable<CategoryViewModel>>> EndpointHandler => (mediator) => mediator.Send(new GetCategoriesQuery());
}

public class GetProductQueryHandler(ApplicationDbContext context) : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryViewModel>>
{
    public async Task<IEnumerable<CategoryViewModel>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await context.Categories
            .ToListAsync(cancellationToken);

        return categories.ConvertAll(s => new CategoryViewModel(s.Id, s.Name));
    }
}

public record CategoryViewModel(string Id, string Name);