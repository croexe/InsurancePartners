/* ===========================================================
   InsurancePartners - jedinstvena skripta za kreiranje baze,
   tablica, i stored procedura. Pokreni CIJELU skriptu jednom,
   u SSMS-u (New Query -> F5), prije prvog pokretanja aplikacije.
   =========================================================== */

-- 1. Kreiranje baze (ako ne postoji)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'WienerPartners')
BEGIN
    CREATE DATABASE WienerPartners;
END
GO

USE WienerPartners;
GO

-- 2. Brisanje starih objekata ako postoje (korisno za development/re-run)
IF OBJECT_ID('dbo.GetAllPartners', 'P') IS NOT NULL DROP PROCEDURE dbo.GetAllPartners;
IF OBJECT_ID('dbo.GetAllPartnersWithPolicySummeries', 'P') IS NOT NULL DROP PROCEDURE dbo.GetAllPartnersWithPolicySummeries;
IF OBJECT_ID('dbo.GetAllPartnersWithPolicySummeriesFirstServe', 'P') IS NOT NULL DROP PROCEDURE dbo.GetAllPartnersWithPolicySummeriesFirstServe;
IF OBJECT_ID('dbo.GetPartnerById', 'P') IS NOT NULL DROP PROCEDURE dbo.GetPartnerById;
IF OBJECT_ID('dbo.PartnerExists', 'P') IS NOT NULL DROP PROCEDURE dbo.PartnerExists;
IF OBJECT_ID('dbo.CreatePartner', 'P') IS NOT NULL DROP PROCEDURE dbo.CreatePartner;
IF OBJECT_ID('dbo.ExternalCodeExists', 'P') IS NOT NULL DROP PROCEDURE dbo.ExternalCodeExists;
IF OBJECT_ID('dbo.GetPoliciesByPartnerId', 'P') IS NOT NULL DROP PROCEDURE dbo.GetPoliciesByPartnerId;
IF OBJECT_ID('dbo.CreatePolicy', 'P') IS NOT NULL DROP PROCEDURE dbo.CreatePolicy;
IF OBJECT_ID('dbo.GetPartnerPolicySummaries', 'P') IS NOT NULL DROP PROCEDURE dbo.GetPartnerPolicySummaries;
IF OBJECT_ID('dbo.GetPolicySummaryByPartnerId', 'P') IS NOT NULL DROP PROCEDURE dbo.GetPolicySummaryByPartnerId;
IF OBJECT_ID('dbo.Policy', 'U') IS NOT NULL DROP TABLE dbo.Policy;
IF OBJECT_ID('dbo.Partner', 'U') IS NOT NULL DROP TABLE dbo.Partner;
GO

-- =========================================================
-- 3. TABLICE
-- =========================================================

CREATE TABLE dbo.Partner
(
    Id              INT IDENTITY(1,1)      NOT NULL,
    FirstName       NVARCHAR(255)          NOT NULL,
    LastName        NVARCHAR(255)          NOT NULL,
    Address         NVARCHAR(MAX)          NULL,
    PartnerNumber   CHAR(20)               NOT NULL,
    CroatianPIN     CHAR(11)               NULL,
    PartnerTypeId   TINYINT                NOT NULL,
    CreatedAtUtc    DATETIME2              NOT NULL CONSTRAINT DF_Partner_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    CreateByUser    NVARCHAR(255)          NOT NULL,
    IsForeign       BIT                    NOT NULL,
    ExternalCode    NVARCHAR(20)           NULL,
    Gender          CHAR(1)                NOT NULL,

    CONSTRAINT PK_Partner PRIMARY KEY (Id),
    CONSTRAINT CK_Partner_FirstName_Length CHECK (LEN(FirstName) >= 2),
    CONSTRAINT CK_Partner_LastName_Length CHECK (LEN(LastName) >= 2),
    CONSTRAINT CK_Partner_PartnerNumber_Digits CHECK (PartnerNumber NOT LIKE '%[^0-9]%'),
    CONSTRAINT CK_Partner_CroatianPIN_Digits CHECK (CroatianPIN IS NULL OR CroatianPIN NOT LIKE '%[^0-9]%'),
    CONSTRAINT CK_Partner_PartnerTypeId CHECK (PartnerTypeId IN (1, 2)),
    CONSTRAINT CK_Partner_Gender CHECK (Gender IN ('M', 'F', 'N')),
    CONSTRAINT CK_Partner_CreateByUser_Email CHECK (CreateByUser LIKE '%_@__%.__%'),
    CONSTRAINT CK_Partner_ExternalCode_Length CHECK (ExternalCode IS NULL OR LEN(ExternalCode) >= 10)
);
GO

-- Filtrirani unique indeks - dozvoljava vise partnera s NULL ExternalCode
CREATE UNIQUE INDEX UQ_Partner_ExternalCode
    ON dbo.Partner (ExternalCode)
    WHERE ExternalCode IS NOT NULL;
GO

CREATE TABLE dbo.Policy
(
    Id              INT IDENTITY(1,1)      NOT NULL,
    PolicyNumber    NVARCHAR(15)           NOT NULL,
    Amount          DECIMAL(18,2)          NOT NULL,
    PartnerId       INT                    NOT NULL,

    CONSTRAINT PK_Policy PRIMARY KEY (Id),
    CONSTRAINT CK_Policy_PolicyNumber_Length CHECK (LEN(PolicyNumber) >= 10),
    CONSTRAINT FK_Policy_Partner FOREIGN KEY (PartnerId)
        REFERENCES dbo.Partner (Id)
        ON DELETE CASCADE
);
GO

CREATE INDEX IX_Partner_CreatedAtUtc ON dbo.Partner (CreatedAtUtc DESC);
CREATE INDEX IX_Policy_PartnerId ON dbo.Policy (PartnerId);
GO

-- =========================================================
-- 4. STORED PROCEDURES - Partner
-- =========================================================

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

CREATE OR ALTER PROCEDURE dbo.GetPartnerById
    @Id INT
AS
BEGIN
    SELECT
        Id, FirstName, LastName, Address, PartnerNumber, CroatianPIN,
        PartnerTypeId, CreatedAtUtc, CreateByUser, IsForeign, ExternalCode, Gender
    FROM dbo.Partner
    WHERE Id = @Id;
END
GO

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

CREATE OR ALTER PROCEDURE dbo.PartnerExists
    @Id INT
AS
BEGIN
    SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.Partner WHERE Id = @Id) THEN 1 ELSE 0 END AS BIT);
END
GO

-- =========================================================
-- 5. STORED PROCEDURES - Policy
-- =========================================================

CREATE OR ALTER PROCEDURE dbo.GetPoliciesByPartnerId
    @PartnerId INT
AS
BEGIN
    SELECT Id, PolicyNumber, Amount, PartnerId
    FROM dbo.Policy
    WHERE PartnerId = @PartnerId;
END
GO

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
GO

PRINT 'Baza, tablice, i stored procedure su uspjesno kreirane.';