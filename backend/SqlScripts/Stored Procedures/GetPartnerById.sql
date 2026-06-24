CREATE OR ALTER PROCEDURE dbo.GetPartnerById
    @Id INT
AS
BEGIN
    SELECT
        Id,
        FirstName,
        LastName,
        Address,
        PartnerNumber,
        CroatianPIN,
        PartnerTypeId,
        CreatedAtUtc,
        CreateByUser,
        IsForeign,
        ExternalCode,
        Gender
    FROM dbo.Partner
    WHERE Id = @Id;
END
GO