using Partners.Core.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Partners.Core.Models;

public class Partner
{
    public int Id { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 2)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 2)]
    public string LastName { get; set; }

    public string? Address { get; set; }

    [Required]
    [RegularExpression(@"^\d{20}$", ErrorMessage = "PartnerNumber must be exactly 20 digits.")]
    public string PartnerNumber { get; set; }

    // OIB - hrvatski osobni identifikacijski broj, neobavezno
    [RegularExpression(@"^\d{11}$", ErrorMessage = "CroatianPIN must be exactly 11 digits.")]
    public string? CroatianPIN { get; set; }

    [Required]
    public PartnerType PartnerTypeId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    [Required]
    [StringLength(255)]
    [EmailAddress]
    public string CreateByUser { get; set; }

    [Required]
    public bool IsForeign { get; set; }

    [StringLength(20, MinimumLength = 10)]
    public string? ExternalCode { get; set; }

    [Required]
    public Gender Gender { get; set; }

    // Navigacijsko svojstvo - polica/e vezane za partnera
    public virtual ICollection<Policy> Policies { get; set; } = new Collection<Policy>();
}