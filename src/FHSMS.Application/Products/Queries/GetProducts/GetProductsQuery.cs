using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Products.Queries.GetProducts;

public record GetProductsQuery(bool ActiveOnly = true) : IRequest<List<ProductDto>>;
