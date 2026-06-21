/* ===========================================================
   InsurancePartners - skripta za kreiranje baze i tablica
   Pokreni cijelu skriptu u SSMS (New Query), F5 za izvrsavanje
   =========================================================== */

-- 1. Kreiranje baze (ako ne postoji)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'WienerPartners')
BEGIN
    CREATE DATABASE InsurancePartners;
END
GO

USE WienerPartners;
GO

-- 2. Brisanje starih tablica ako postoje (korisno za development/re-run)
IF OBJECT_ID('dbo.Policy', 'U') IS NOT NULL DROP TABLE dbo.Policy;
IF OBJECT_ID('dbo.Partner', 'U') IS NOT NULL DROP TABLE dbo.Partner;
GO

-- 3. Tablica Partner
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

    -- PartnerNumber: tocno 20 znamenki (samo brojevi)
    CONSTRAINT CK_Partner_PartnerNumber_Digits CHECK (PartnerNumber NOT LIKE '%[^0-9]%'),

    -- CroatianPIN (OIB): ako je upisan, mora imati tocno 11 znamenki
    CONSTRAINT CK_Partner_CroatianPIN_Digits CHECK (CroatianPIN IS NULL OR CroatianPIN NOT LIKE '%[^0-9]%'),

    -- PartnerTypeId: samo 1 (Personal) ili 2 (Legal)
    CONSTRAINT CK_Partner_PartnerTypeId CHECK (PartnerTypeId IN (1, 2)),

    -- Gender: samo M, F ili N
    CONSTRAINT CK_Partner_Gender CHECK (Gender IN ('M', 'F', 'N')),

    -- CreateByUser: mora sadrzavati '@' (osnovna provjera formata e-maila)
    CONSTRAINT CK_Partner_CreateByUser_Email CHECK (CreateByUser LIKE '%_@__%.__%'),

    -- ExternalCode: ako je upisan, mora imati najmanje 10 znakova (jedinstvenost se
    -- provodi kroz filtrirani unique indeks ispod, ne kroz standardni UNIQUE constraint)
    CONSTRAINT CK_Partner_ExternalCode_Length CHECK (ExternalCode IS NULL OR LEN(ExternalCode) >= 10)
);
GO

-- 3a. Filtrirani unique indeks za ExternalCode.
-- Standardni UNIQUE constraint u SQL Serveru dozvoljava SAMO JEDAN NULL u koloni -
-- drugi NULL bi bio tretiran kao duplikat, sto je pogresno za neobavezno polje
-- gdje vise partnera legitimno nema ExternalCode. Filtrirani indeks (WHERE ExternalCode
-- IS NOT NULL) provjerava jedinstvenost SAMO kad je vrijednost upisana.
CREATE UNIQUE INDEX UQ_Partner_ExternalCode
    ON dbo.Partner (ExternalCode)
    WHERE ExternalCode IS NOT NULL;
GO

-- 4. Tablica Policy
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

-- 5. Indeksi za uobicajene upite
CREATE INDEX IX_Partner_CreatedAtUtc ON dbo.Partner (CreatedAtUtc DESC);
CREATE INDEX IX_Policy_PartnerId ON dbo.Policy (PartnerId);
GO

PRINT 'Baza i tablice su uspjesno kreirane.';