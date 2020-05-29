﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Application.Products.Models;
using BShop.API.Data;
using BShop.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BShop.API.Application.Products.Queries.GetAll
{
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
    {
        private readonly IRepository<Product> _repository;

        public GetAllProductsQueryHandler(IRepository<Product> repository)
        {
            _repository = repository;
        }

        public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.Query()
                .Select(s => new ProductDto
                { 
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    ImageUri = s.ImageUri,
                    Description = s.Description,
                    Categories = s.ProductCategories.Select(c => new CategoryDto
                    {
                        Id = c.CategoryId,
                        Name = c.Category.Name
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            return result;
        }
    }
}
