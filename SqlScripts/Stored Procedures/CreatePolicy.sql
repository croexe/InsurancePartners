CREATE OR ALTER PROCEDURE dbo.CreatePolicy
    @PolicyNumber NVARCHAR(15),
    @Amount       DECIMAL(18,2),
    @PartnerId    INT
AS
BEGIN
    INSERT INTO dbo.Policy (PolicyNumber, Amount, PartnerId)
    OUTPUT INSERTED.Id
    VALUES (@PolicyNumber, @Amount, @PartnerId);
END
GO