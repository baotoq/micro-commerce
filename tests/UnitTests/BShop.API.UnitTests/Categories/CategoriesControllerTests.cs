using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Categories;
using BShop.API.Categories.Commands.Create;
using BShop.API.Categories.Commands.Delete;
using BShop.API.Categories.Commands.Put;
using BShop.API.Categories.Models;
using BShop.API.Categories.Queries.GetAll;
using BShop.API.Categories.Queries.GetById;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BShop.API.UnitTests.Categories
{
    public class CategoriesControllerTests
    {
        private readonly CategoriesController _sut;
        private readonly Mock<IMediator> _mockMediator;

        public CategoriesControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _sut = new CategoriesController(_mockMediator.Object);
        }

        [Fact]
        public async Task GetAllAsync_Success()
        {
            _mockMediator
                .Setup(_ => _.Send(It.IsAny<GetAllCategoriesQuery>(), CancellationToken.None))
                .ReturnsAsync(new List<CategoryDto>
                {
                    new CategoryDto(),
                    new CategoryDto()
                });

            var act = await _sut.GetAllAsync(CancellationToken.None);

            act.Value.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAsync_Success()
        {
            var dto = new CategoryDto
            {
                Id = 1,
                Name = "test"
            };

            _mockMediator
                .Setup(_ => _.Send(It.IsAny<GetCategoryByIdQuery>(), CancellationToken.None))
                .ReturnsAsync(dto);

            var act = await _sut.GetAsync(1, CancellationToken.None);

            act.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task PostAsync_Success()
        {
            var dto = new CategoryDto
            {
                Id = 1,
                Name = "test"
            };

            _mockMediator
                .Setup(_ => _.Send(It.IsAny<CreateCategoryCommand>(), CancellationToken.None))
                .ReturnsAsync(dto);

            var act = await _sut.PostAsync(new CreateCategoryCommand
            {
                Name = "test"
            }, CancellationToken.None);

            act.Result.Should().BeOfType<CreatedAtActionResult>()
                .Subject.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task PutAsync_Success()
        {
            var dto = new CategoryDto
            {
                Id = 1,
                Name = "test"
            };

            _mockMediator
                .Setup(_ => _.Send(It.IsAny<PutCategoryCommand>(), CancellationToken.None))
                .ReturnsAsync(dto);

            var act = await _sut.PutAsync(1, new PutCategoryCommand
            {
                Name = "test"
            }, CancellationToken.None);

            act.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteAsync_Success()
        {
            _mockMediator
                .Setup(_ => _.Send(It.IsAny<DeleteCategoryCommand>(), CancellationToken.None))
                .ReturnsAsync(true);

            var act = await _sut.DeleteAsync(1, CancellationToken.None);

            act.Should().BeOfType<NoContentResult>();
        }
    }
}
