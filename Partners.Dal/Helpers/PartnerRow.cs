using Partners.Core.Models;
using Partners.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Partners.Dal.Helpers
{
    internal sealed class PartnerRow
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string? Address { get; init; }
        public string PartnerNumber { get; init; } = string.Empty;
        public string? CroatianPIN { get; init; }
        public int PartnerTypeId { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public string CreateByUser { get; init; } = string.Empty;
        public bool IsForeign { get; init; }
        public string? ExternalCode { get; init; }
        public string Gender { get; init; } = string.Empty;

        public Partner ToPartner() => new()
        {
            Id = Id,
            FirstName = FirstName,
            LastName = LastName,
            Address = Address,
            PartnerNumber = PartnerNumber,
            CroatianPIN = CroatianPIN,
            PartnerTypeId = (PartnerType)PartnerTypeId,
            CreatedAtUtc = CreatedAtUtc,
            CreateByUser = CreateByUser,
            IsForeign = IsForeign,
            ExternalCode = ExternalCode,
            Gender = Enum.Parse<Gender>(Gender, ignoreCase: true)
        };
    }
}
