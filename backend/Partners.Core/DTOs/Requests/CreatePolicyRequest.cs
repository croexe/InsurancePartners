namespace Partners.Core.DTOs.Requests;

public sealed class CreatePolicyRequest
{
    public string PolicyNumber { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public int? PartnerId { get; set; }
}
