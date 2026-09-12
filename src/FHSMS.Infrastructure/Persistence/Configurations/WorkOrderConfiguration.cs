using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHSMS.Infrastructure.Persistence.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Title).IsRequired().HasMaxLength(200);
        builder.Property(w => w.Description).HasMaxLength(1000);
        builder.Property(w => w.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(w => w.Priority).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(w => w.AssignedToUserId);
        builder.HasIndex(w => w.OrderId);
    }
}
