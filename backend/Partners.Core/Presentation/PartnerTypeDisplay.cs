using Partners.Core.Models.Enums;

namespace Partners.Core.Presentation;

// Prezentacijski (display) nazivi tipa partnera — izdvojeni iz poslovne logike,
// jedinstveni seam za lokalizaciju ili premjestanje u API sloj.
public static class PartnerTypeDisplay
{
    public static string ToDisplayName(this PartnerType partnerType) =>
        partnerType == PartnerType.Personal ? "Privatna osoba" : "Pravna osoba";
}
