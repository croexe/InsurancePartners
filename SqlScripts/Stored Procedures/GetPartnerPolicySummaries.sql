CREATE OR ALTER PROCEDURE dbo.GetPartnerPolicySummaries
AS
BEGIN
    SELECT
        PartnerId,
        COUNT(1)               AS PolicyCount,
        ISNULL(SUM(Amount), 0) AS TotalAmount
    FROM dbo.Policy
    GROUP BY PartnerId;
END
GO