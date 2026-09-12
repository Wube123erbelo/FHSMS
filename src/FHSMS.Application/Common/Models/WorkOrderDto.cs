using FHSMS.Domain.Enums;

namespace FHSMS.Application.Common.Models;

public class WorkOrderDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public WorkOrderStatus Status { get; set; }
    public WorkOrderPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
