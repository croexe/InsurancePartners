using System.ComponentModel.DataAnnotations;

namespace Partners.Core.Models
{
    public sealed class Policy
    {
        public int Id { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int PartnerId { get; set; }
        public Partner? Partner { get; set; }
    }
}
