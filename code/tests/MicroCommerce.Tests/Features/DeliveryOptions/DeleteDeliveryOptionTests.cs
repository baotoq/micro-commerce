using System;
using System.Threading;
using System.Threading.Tasks;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Features.DeliveryOptions;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MicroCommerce.Tests.Features.DeliveryOptions;

public class DeleteDeliveryOptionTests : TestBase
{
    [Fact]
    public async Task DeleteDeliveryOption_Removes_Entity_When_Exists()
    {
        // Arrange
        var deliveryOption = new DeliveryOption
        {
            Id = Guid.NewGuid(),
            Name = "Test Option",
            MinimumSpending = 100,
            Fee = 10
        };

        await SeedContext.DeliveryOptions.AddAsync(deliveryOption);
        await SeedContext.SaveChangesAsync();

        var command = new DeleteDeliveryOption.Command { Id = deliveryOption.Id };
        var handler = new DeleteDeliveryOption.Handler(VerifyContext);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedOption = await VerifyContext.DeliveryOptions.FirstOrDefaultAsync(x => x.Id == deliveryOption.Id);

        await Verify(deletedOption);
    }

    [Fact]
    public async Task DeleteDeliveryOption_Throws_NotFoundException_When_Not_Exists()
    {
        // Arrange
        var command = new DeleteDeliveryOption.Command { Id = Guid.NewGuid() };
        var handler = new DeleteDeliveryOption.Handler(VerifyContext);

        // Act
        // Assert
        await ThrowsTask(() => handler.Handle(command, CancellationToken.None));
    }
}
