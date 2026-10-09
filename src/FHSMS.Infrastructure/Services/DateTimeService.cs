using FHSMS.Application.Common.Interfaces;

namespace FHSMS.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}
