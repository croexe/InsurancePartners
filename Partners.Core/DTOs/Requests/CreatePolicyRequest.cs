using System.ComponentModel.DataAnnotations;

namespace Partners.Core.DTOs.Requests
{
    public class CreatePolicyRequest
    {
        [Required]
        [StringLength(15, MinimumLength = 10)]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal? Amount { get; set; }

        [Required(ErrorMessage = "PartnerId is required.")]
        public int? PartnerId { get; set; }
    }
}
