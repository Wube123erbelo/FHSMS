using FHSMS.Domain.Common;
using FHSMS.Domain.Enums;
using FHSMS.Domain.Exceptions;

namespace FHSMS.Domain.Entities;

/// <summary>
/// An internal operational task - "prepare order #123 for dispatch", "collect
/// stock from Farmer X", "restock check for Category Y". Optionally linked to
/// an Order, but stands alone otherwise (e.g. a farm pickup task has no order
/// yet). This is the concrete implementation of the original design doc's
/// bare "Work Order" concept for coordinating staff/agent tasks.
/// </summary>
public class WorkOrder : AuditableEntity
{
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid? OrderId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public WorkOrderStatus Status { get; private set; }
    public WorkOrderPriority Priority { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private WorkOrder() { } // EF Core

    public WorkOrder(string title, string? description, Guid? orderId, Guid? assignedToUserId, WorkOrderPriority priority, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Work order title is required.");

        Title = title;
        Description = description;
        OrderId = orderId;
        AssignedToUserId = assignedToUserId;
        Priority = priority;
        DueDate = dueDate;
        Status = WorkOrderStatus.Pending;
    }

    public void Reassign(Guid? assignedToUserId) => AssignedToUserId = assignedToUserId;

    public void Start()
    {
        if (Status != WorkOrderStatus.Pending)
            throw new DomainException("Only a pending work order can be started.");
        Status = WorkOrderStatus.InProgress;
    }

    public void Complete()
    {
        if (Status != WorkOrderStatus.InProgress && Status != WorkOrderStatus.Pending)
            throw new DomainException("Only a pending or in-progress work order can be completed.");
        Status = WorkOrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel() => Status = WorkOrderStatus.Cancelled;
}
