CREATE OR ALTER PROCEDURE dbo.GetPoliciesByPartnerId
    @PartnerId INT
AS
BEGIN
    SELECT Id, PolicyNumber, Amount, PartnerId
    FROM dbo.Policy
    WHERE PartnerId = @PartnerId;
END
GO