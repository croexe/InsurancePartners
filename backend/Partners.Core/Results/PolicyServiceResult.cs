using Partners.Core.DTOs.Responses;

namespace Partners.Core.Results
{
    public class PolicyServiceResult
    {
        public bool Success { get; set; }
        public PolicyResponse? Policy { get; set; }
        public List<string> Errors { get; set; } = [];

        public static PolicyServiceResult Ok(PolicyResponse policy) =>
            new() { Success = true, Policy = policy };

        public static PolicyServiceResult Fail(params string[] errors) =>
            new() { Success = false, Errors = errors.ToList() };
    }
}
