namespace FHSMS.Application.Common.Models;

public class UnitDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Abbreviation { get; set; } = default!;
    public bool IsActive { get; set; }
}
