CREATE OR ALTER PROCEDURE dbo.GetAllPartnersWithPolicySummeriesFirstServe
    @Offset   INT,
    @PageSize INT
AS
BEGIN
    SELECT
        p.Id, p.FirstName, p.LastName, p.Address, p.PartnerNumber, p.CroatianPIN,
        p.PartnerTypeId, p.CreatedAtUtc, p.CreateByUser, p.IsForeign, p.ExternalCode, p.Gender,
        ISNULL(ps.PolicyCount, 0) AS PolicyCount,
        ISNULL(ps.TotalAmount, 0) AS TotalAmount
    FROM dbo.Partner p
    LEFT JOIN (
        SELECT PartnerId, COUNT(*) AS PolicyCount, SUM(Amount) AS TotalAmount
        FROM dbo.Policy
        GROUP BY PartnerId
    ) ps ON ps.PartnerId = p.Id
    ORDER BY p.CreatedAtUtc DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) FROM dbo.Partner;
END
GO
