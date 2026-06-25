namespace Partners.Core.Models
{
    public class PartnerWithSummary
    {
        public Partner Partner { get; init; } = null!;
        public int PolicyCount { get; init; }
        public decimal TotalAmount { get; init; }
    }
}
