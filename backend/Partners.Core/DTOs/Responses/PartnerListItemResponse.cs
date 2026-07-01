namespace Partners.Core.DTOs.Responses;

public sealed class PartnerListItemResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PartnerNumber { get; set; } = string.Empty;
    public string? CroatianPIN { get; set; }
    public string PartnerTypeName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public bool IsForeign { get; set; }
    public string Gender { get; set; } = string.Empty;
    public bool IsFlagged { get; set; } // true = treba * prije imena (>5 polica ili >5000kn)
}
