namespace Partners.Core.Results
{
    public class PartnerServiceResult
    {
        public bool Success { get; set; }
        public int? PartnerId { get; set; }
        public List<string> Errors { get; set; } = [];

        public static PartnerServiceResult Ok(int partnerId) =>
            new() { Success = true, PartnerId = partnerId };

        public static PartnerServiceResult Fail(params string[] errors) =>
            new() { Success = false, Errors = errors.ToList() };
    }
}
