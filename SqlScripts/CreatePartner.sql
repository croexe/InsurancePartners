CREATE OR ALTER PROCEDURE dbo.CreatePartner
    @FirstName     NVARCHAR(255),
    @LastName      NVARCHAR(255),
    @Address       NVARCHAR(MAX),
    @PartnerNumber CHAR(20),
    @CroatianPIN   CHAR(11),
    @PartnerTypeId TINYINT,
    @CreateByUser  NVARCHAR(255),
    @IsForeign     BIT,
    @ExternalCode  NVARCHAR(20),
    @Gender        CHAR(1)
AS
BEGIN
    INSERT INTO dbo.Partner
        (FirstName, LastName, Address, PartnerNumber, CroatianPIN,
         PartnerTypeId, CreateByUser, IsForeign, ExternalCode, Gender)
    OUTPUT INSERTED.Id
    VALUES
        (@FirstName, @LastName, @Address, @PartnerNumber, @CroatianPIN,
         @PartnerTypeId, @CreateByUser, @IsForeign, @ExternalCode, @Gender);
END
GO