namespace Partners.Core.DTOs.Responses;

public sealed class PolicyResponse
{
    public int Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int PartnerId { get; set; }
}
