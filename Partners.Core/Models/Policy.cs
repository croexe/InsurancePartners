using System.ComponentModel.DataAnnotations;

namespace Partners.Core.Models
{
    public sealed class Policy
    {
        public int Id { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 10)]
        public string PolicyNumber { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public int PartnerId { get; set; }

        // Navigacijsko svojstvo - partner kojem polica pripada
        public Partner? Partner { get; set; }
    }
}
