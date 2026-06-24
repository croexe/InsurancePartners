using Partners.Core.Models.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Partners.Core.Models;

public class Partner
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string PartnerNumber { get; set; } = string.Empty;
    public string? CroatianPIN { get; set; }
    public PartnerType PartnerTypeId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string CreateByUser { get; set; } = string.Empty;
    public bool IsForeign { get; set; }
    public string? ExternalCode { get; set; }
    public Gender Gender { get; set; }
    public virtual ICollection<Policy> Policies { get; set; } = new Collection<Policy>();
    public int PolicyCount { get; set; }
    public decimal TotalAmount { get; set; }
}