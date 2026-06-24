using Partners.Core.Models.Enums;

namespace Partners.Core.DTOs.Requests;

public sealed class CreatePartnerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string PartnerNumber { get; set; } = string.Empty;
    public string? CroatianPIN { get; set; }
    public PartnerType? PartnerTypeId { get; set; }
    public string CreateByUser { get; set; } = string.Empty;
    public bool? IsForeign { get; set; }
    public string? ExternalCode { get; set; }
    public Gender? Gender { get; set; }
}
