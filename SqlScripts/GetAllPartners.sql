CREATE OR ALTER PROCEDURE dbo.GetAllPartners
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
    ORDER BY CreatedAtUtc DESC;
END
GO