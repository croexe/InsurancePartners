namespace Partners.Core.DTOs.Responses;

public sealed class PartnerDetailResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string PartnerNumber { get; set; } = string.Empty;
    public string? CroatianPIN { get; set; }
    public string PartnerTypeName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string CreateByUser { get; set; } = string.Empty;
    public bool IsForeign { get; set; }
    public string? ExternalCode { get; set; }
    public string Gender { get; set; } = string.Empty;
    public bool IsFlagged { get; set; }
    public List<PolicyResponse> Policies { get; set; } = [];
}
