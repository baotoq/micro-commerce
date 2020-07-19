using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.ApiControllers;
using Catalog.API.Application.Categories.Commands;
using Catalog.API.Application.Categories.Commands.Create;
using Catalog.API.Application.Categories.Models;
using Catalog.API.Application.Categories.Queries;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnitOfWork.Common;
using Xunit;

namespace Catalog.API.UnitTests.Categories
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
        public async Task Find_Success()
        {
            _mockMediator
                .Setup(_ => _.Send(It.IsAny<FindCategoriesQuery>(), CancellationToken.None))
                .ReturnsAsync(new OffsetPaged<CategoryDto>
                {
                    Data = new List<CategoryDto>
                    {
                        new CategoryDto(),
                        new CategoryDto()
                    }
                });

            var act = await _sut.FindCategories(new FindCategoriesQuery(), CancellationToken.None);

            act.Value.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task FindById_Success()
        {
            var dto = new CategoryDto
            {
                Id = 1,
                Name = "test"
            };

            _mockMediator
                .Setup(_ => _.Send(It.IsAny<FindCategoryByIdQuery>(), CancellationToken.None))
                .ReturnsAsync(dto);

            var act = await _sut.FindCategoryById(1, CancellationToken.None);

            act.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task Create_Success()
        {
            var dto = new CategoryDto
            {
                Id = 1,
                Name = "test"
            };

            _mockMediator
                .Setup(_ => _.Send(It.IsAny<CreateCategoryCommand>(), CancellationToken.None))
                .ReturnsAsync(dto);

            var act = await _sut.CreateCategory(new CreateCategoryCommand
            {
                Name = "test"
            }, CancellationToken.None);

            act.Result.Should().BeOfType<CreatedAtActionResult>()
                .Subject.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task Put_Success()
        {
            _mockMediator
                .Setup(_ => _.Send(It.IsAny<UpdateCategoryCommand>(), CancellationToken.None))
                .ReturnsAsync(Unit.Value);

            var act = await _sut.UpdateCategory(1, new UpdateCategoryCommand
            {
                Name = "test"
            }, CancellationToken.None);

            act.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_Success()
        {
            _mockMediator
                .Setup(_ => _.Send(It.IsAny<DeleteCategoryCommand>(), CancellationToken.None))
                .ReturnsAsync(Unit.Value);

            var act = await _sut.DeleteCategory(1, CancellationToken.None);

            act.Should().BeOfType<NoContentResult>();
        }
    }
}
