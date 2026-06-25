using Partners.Core.DTOs.Responses;

namespace Partners.Core.Results
{
    public class PolicyServiceResult
    {
        public bool Success { get; set; }
        public PolicyResponse? Policy { get; set; }
        public bool IsFlagged { get; set; }
        public List<string> Errors { get; set; } = [];

        public static PolicyServiceResult Ok(PolicyResponse policy, bool isFlagged) =>
            new() { Success = true, Policy = policy, IsFlagged = isFlagged };

        public static PolicyServiceResult Fail(params string[] errors) =>
            new() { Success = false, Errors = errors.ToList() };
    }
}
