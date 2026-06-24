CREATE OR ALTER PROCEDURE dbo.GetPolicySummaryByPartnerId
    @PartnerId INT
AS
BEGIN
    SELECT
        @PartnerId             AS PartnerId,
        COUNT(*)               AS PolicyCount,
        ISNULL(SUM(Amount), 0) AS TotalAmount
    FROM dbo.Policy
    WHERE PartnerId = @PartnerId;
END