using Partners.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Partners.Core.DTOs.Requests
{
    public sealed class CreatePartnerRequest
    {
        [Required]
        [StringLength(255, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z0-9\u00C0-\u017F ]+$", ErrorMessage = "FirstName must be alphanumeric.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(255, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z0-9\u00C0-\u017F ]+$", ErrorMessage = "LastName must be alphanumeric.")]
        public string LastName { get; set; } = string.Empty;

        [RegularExpression(@"^[a-zA-Z0-9\u00C0-\u017F ,.\-]+$", ErrorMessage = "Address must be alphanumeric.")]
        public string? Address { get; set; }

        [Required]
        [RegularExpression(@"^\d{20}$", ErrorMessage = "PartnerNumber must be exactly 20 digits.")]
        public string PartnerNumber { get; set; } = string.Empty;

        [RegularExpression(@"^\d{11}$", ErrorMessage = "CroatianPIN must be exactly 11 digits.")]
        public string? CroatianPIN { get; set; }

        [Required(ErrorMessage = "PartnerTypeId is required.")]
        public PartnerType? PartnerTypeId { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string CreateByUser { get; set; } = string.Empty;

        [Required(ErrorMessage = "IsForeign is required.")]
        public bool? IsForeign { get; set; }

        [StringLength(20, MinimumLength = 10)]
        public string? ExternalCode { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public Gender? Gender { get; set; }
    }
}
