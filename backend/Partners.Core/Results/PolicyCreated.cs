using Partners.Core.DTOs.Responses;

namespace Partners.Core.Results;

public sealed record PolicyCreated(PolicyResponse Policy, bool IsFlagged);
