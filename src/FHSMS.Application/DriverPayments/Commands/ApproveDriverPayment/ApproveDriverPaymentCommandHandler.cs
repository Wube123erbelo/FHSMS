using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.DriverPayments.Commands.ApproveDriverPayment;

public class ApproveDriverPaymentCommandHandler : IRequestHandler<ApproveDriverPaymentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ApproveDriverPaymentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(ApproveDriverPaymentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Must be logged in to approve a driver payment.");

        var payment = await _context.DriverPayments
            .FirstOrDefaultAsync(p => p.Id == request.DriverPaymentId, cancellationToken)
            ?? throw new NotFoundException(nameof(DriverPayment), request.DriverPaymentId);

        payment.Approve(userId, request.DriverWasPaid);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
