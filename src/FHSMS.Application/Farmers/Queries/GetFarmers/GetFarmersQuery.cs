using FHSMS.Application.Common.Models;
using MediatR;

namespace FHSMS.Application.Farmers.Queries.GetFarmers;

public record GetFarmersQuery : IRequest<List<FarmerDto>>;
