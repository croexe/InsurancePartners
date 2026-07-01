using Partners.Core.Models;
using Partners.Core.Models.Enums;

namespace Partners.Dal.Helpers;

internal sealed class PartnerRow
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Address { get; init; }
    public string PartnerNumber { get; init; } = string.Empty;
    public string? CroatianPIN { get; init; }
    public int PartnerTypeId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public string CreateByUser { get; init; } = string.Empty;
    public bool IsForeign { get; init; }
    public string? ExternalCode { get; init; }
    public string Gender { get; init; } = string.Empty;
    public int PolicyCount { get; init; }
    public decimal TotalAmount { get; init; }

    public Partner ToPartner() => new()
    {
        Id = Id,
        FirstName = FirstName,
        LastName = LastName,
        Address = Address,
        PartnerNumber = PartnerNumber,
        CroatianPIN = CroatianPIN,
        PartnerTypeId = (PartnerType)PartnerTypeId,
        CreatedAtUtc = CreatedAtUtc,
        CreateByUser = CreateByUser,
        IsForeign = IsForeign,
        ExternalCode = ExternalCode,
        Gender = Enum.Parse<Gender>(Gender, ignoreCase: true)
    };

    public PartnerWithSummary ToPartnerWithSummary() => new()
    {
        Partner = ToPartner(),
        PolicyCount = PolicyCount,
        TotalAmount = TotalAmount
    };
}
