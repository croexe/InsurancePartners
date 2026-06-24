CREATE OR ALTER PROCEDURE dbo.ExternalCodeExists
    @ExternalCode NVARCHAR(20)
AS
BEGIN
    SELECT COUNT(1)
    FROM dbo.Partner
    WHERE ExternalCode = @ExternalCode;
END
GO