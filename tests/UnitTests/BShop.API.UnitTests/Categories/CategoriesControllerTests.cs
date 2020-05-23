using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Application.Categories;
using BShop.API.Application.Categories.Commands.Create;
using BShop.API.Application.Categories.Commands.Delete;
using BShop.API.Application.Categories.Commands.Put;
using BShop.API.Application.Categories.Models;
using BShop.API.Application.Categories.Queries.GetAll;
using BShop.API.Application.Categories.Queries.GetById;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
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
            _sut = new CategoriesController(_mockMediator.Object, NullLogger<CategoriesController>.Instance);
        }

        [Fact]
        public async Task GetAll_Success()
        {
            _mockMediator
                .Setup(_ => _.Send(It.IsAny<GetAllCategoriesQuery>(), CancellationToken.None))
                .ReturnsAsync(new List<CategoryDto>
                {
                    new CategoryDto(),
                    new CategoryDto()
                });

            var act = await _sut.GetAll(CancellationToken.None);

            act.Value.Should().HaveCount(2);
        }

        [Fact]
        public async Task Get_Success()
        {
            var dto = new CategoryDto
            {
                Id = 1,
                Name = "test"
            };

            _mockMediator
                .Setup(_ => _.Send(It.IsAny<GetCategoryByIdQuery>(), CancellationToken.None))
                .ReturnsAsync(dto);

            var act = await _sut.Get(1, CancellationToken.None);

            act.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task Post_Success()
        {
            var dto = new CategoryDto
            {
                Id = 1,
                Name = "test"
            };

            _mockMediator
                .Setup(_ => _.Send(It.IsAny<CreateCategoryCommand>(), CancellationToken.None))
                .ReturnsAsync(dto);

            var act = await _sut.Post(new CreateCategoryCommand
            {
                Name = "test"
            }, CancellationToken.None);

            act.Result.Should().BeOfType<CreatedAtActionResult>()
                .Subject.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task Put_Success()
        {
            var dto = new CategoryDto
            {
                Id = 1,
                Name = "test"
            };

            _mockMediator
                .Setup(_ => _.Send(It.IsAny<PutCategoryCommand>(), CancellationToken.None))
                .ReturnsAsync(dto);

            var act = await _sut.Put(1, new PutCategoryCommand
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
                .ReturnsAsync(true);

            var act = await _sut.Delete(1, CancellationToken.None);

            act.Should().BeOfType<NoContentResult>();
        }
    }
}
