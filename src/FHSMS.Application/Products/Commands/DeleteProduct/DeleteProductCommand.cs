using MediatR;

namespace FHSMS.Application.Products.Commands.DeleteProduct;

/// <summary>
/// Soft-deletes a product (IsActive = false). A hard delete would break
/// referential integrity for any OrderItem/InvoiceItem that already
/// references this product's Id - historical orders and invoices must keep
/// showing the product they were actually placed against.
/// </summary>
public record DeleteProductCommand(Guid ProductId) : IRequest;
