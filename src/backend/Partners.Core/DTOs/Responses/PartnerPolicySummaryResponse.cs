namespace Partners.Core.DTOs.Responses
{
    public record PartnerPolicySummaryResponse(int PartnerId, int PolicyCount, decimal TotalAmount);
}
