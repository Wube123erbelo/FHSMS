using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Orders.Commands.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CancelOrderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        // An agent may only cancel an order they themselves placed - the
        // [Authorize(Roles = "...")] attribute on the endpoint can express
        // "which roles", but not "only your own", so that check lives here.
        // SuperAdmin can always cancel any order.
        var isAgent = _currentUser.Role is "HotelAgent" or "FarmerAgent";
        if (isAgent && order.AgentUserId != _currentUser.UserId)
            throw new UnauthorizedAccessException("You can only cancel orders you placed yourself.");

        order.Cancel();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
