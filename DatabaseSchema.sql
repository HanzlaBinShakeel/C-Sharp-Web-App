-- =============================================
-- Finance App Database Schema Script
-- SQL Server Database Schema
-- Generated from Entity Framework Core Models
-- =============================================

USE master;
GO

-- Create Database (if it doesn't exist)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'FinanceApp')
BEGIN
    CREATE DATABASE FinanceApp;
END
GO

USE FinanceApp;
GO

-- =============================================
-- Drop Existing Tables (in reverse order of dependencies)
-- =============================================
IF OBJECT_ID('BankReconciliationItems', 'U') IS NOT NULL DROP TABLE BankReconciliationItems;
IF OBJECT_ID('BankReconciliations', 'U') IS NOT NULL DROP TABLE BankReconciliations;
IF OBJECT_ID('ScheduleItems', 'U') IS NOT NULL DROP TABLE ScheduleItems;
IF OBJECT_ID('Schedules', 'U') IS NOT NULL DROP TABLE Schedules;
IF OBJECT_ID('JournalEntryLines', 'U') IS NOT NULL DROP TABLE JournalEntryLines;
IF OBJECT_ID('JournalEntries', 'U') IS NOT NULL DROP TABLE JournalEntries;
IF OBJECT_ID('LedgerBalances', 'U') IS NOT NULL DROP TABLE LedgerBalances;
IF OBJECT_ID('VoucherNumbers', 'U') IS NOT NULL DROP TABLE VoucherNumbers;
IF OBJECT_ID('FinancialYears', 'U') IS NOT NULL DROP TABLE FinancialYears;
IF OBJECT_ID('Vendors', 'U') IS NOT NULL DROP TABLE Vendors;
IF OBJECT_ID('Customers', 'U') IS NOT NULL DROP TABLE Customers;
IF OBJECT_ID('Ledgers', 'U') IS NOT NULL DROP TABLE Ledgers;
IF OBJECT_ID('AccountGroups', 'U') IS NOT NULL DROP TABLE AccountGroups;
IF OBJECT_ID('AuditLogs', 'U') IS NOT NULL DROP TABLE AuditLogs;
IF OBJECT_ID('AspNetUserTokens', 'U') IS NOT NULL DROP TABLE AspNetUserTokens;
IF OBJECT_ID('AspNetUserRoles', 'U') IS NOT NULL DROP TABLE AspNetUserRoles;
IF OBJECT_ID('AspNetUserLogins', 'U') IS NOT NULL DROP TABLE AspNetUserLogins;
IF OBJECT_ID('AspNetUserClaims', 'U') IS NOT NULL DROP TABLE AspNetUserClaims;
IF OBJECT_ID('AspNetRoles', 'U') IS NOT NULL DROP TABLE AspNetRoles;
IF OBJECT_ID('AspNetUsers', 'U') IS NOT NULL DROP TABLE AspNetUsers;
GO

-- =============================================
-- Create ASP.NET Identity Tables
-- =============================================

-- AspNetUsers
CREATE TABLE AspNetUsers (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    UserName NVARCHAR(256) NULL,
    NormalizedUserName NVARCHAR(256) NULL,
    Email NVARCHAR(256) NULL,
    NormalizedEmail NVARCHAR(256) NULL,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    PasswordHash NVARCHAR(MAX) NULL,
    SecurityStamp NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    PhoneNumber NVARCHAR(MAX) NULL,
    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    LockoutEnd DATETIMEOFFSET NULL,
    LockoutEnabled BIT NOT NULL DEFAULT 0,
    AccessFailedCount INT NOT NULL DEFAULT 0
);
GO

-- AspNetRoles
CREATE TABLE AspNetRoles (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    Name NVARCHAR(256) NULL,
    NormalizedName NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL
);
GO

-- AspNetUserClaims
CREATE TABLE AspNetUserClaims (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

-- AspNetUserLogins
CREATE TABLE AspNetUserLogins (
    LoginProvider NVARCHAR(128) NOT NULL,
    ProviderKey NVARCHAR(128) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX) NULL,
    UserId NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

-- AspNetUserRoles
CREATE TABLE AspNetUserRoles (
    UserId NVARCHAR(450) NOT NULL,
    RoleId NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);
GO

-- AspNetUserTokens
CREATE TABLE AspNetUserTokens (
    UserId NVARCHAR(450) NOT NULL,
    LoginProvider NVARCHAR(128) NOT NULL,
    Name NVARCHAR(128) NOT NULL,
    Value NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
GO

-- =============================================
-- Create Application Tables
-- =============================================

-- AccountGroups
CREATE TABLE AccountGroups (
    GroupID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    GroupCode NVARCHAR(50) NOT NULL,
    GroupName NVARCHAR(200) NOT NULL,
    GroupType NVARCHAR(50) NOT NULL,
    ParentGroupID INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_AccountGroups_ParentGroup FOREIGN KEY (ParentGroupID) REFERENCES AccountGroups(GroupID) ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX IX_AccountGroups_GroupCode ON AccountGroups(GroupCode);
GO

-- Ledgers
CREATE TABLE Ledgers (
    LedgerID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    LedgerCode NVARCHAR(50) NOT NULL,
    LedgerName NVARCHAR(200) NOT NULL,
    GroupID INT NOT NULL,
    OpeningBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    OpeningBalanceType NVARCHAR(10) NOT NULL DEFAULT 'Debit',
    IsActive BIT NOT NULL DEFAULT 1,
    Address NVARCHAR(500) NULL,
    ContactInfo NVARCHAR(100) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Ledgers_AccountGroups FOREIGN KEY (GroupID) REFERENCES AccountGroups(GroupID) ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX IX_Ledgers_LedgerCode ON Ledgers(LedgerCode);
GO

-- FinancialYears
CREATE TABLE FinancialYears (
    FinancialYearID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    YearName NVARCHAR(50) NOT NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    IsActive BIT NOT NULL DEFAULT 0,
    IsClosed BIT NOT NULL DEFAULT 0,
    Notes NVARCHAR(500) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(450) NULL
);
GO

-- Vendors
CREATE TABLE Vendors (
    VendorID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    VendorCode NVARCHAR(50) NOT NULL,
    VendorName NVARCHAR(200) NOT NULL,
    ContactPerson NVARCHAR(200) NULL,
    Email NVARCHAR(100) NULL,
    Phone NVARCHAR(20) NULL,
    Mobile NVARCHAR(20) NULL,
    Address NVARCHAR(500) NULL,
    City NVARCHAR(100) NULL,
    State NVARCHAR(50) NULL,
    ZipCode NVARCHAR(20) NULL,
    Country NVARCHAR(50) NULL,
    TaxID NVARCHAR(50) NULL,
    BankName NVARCHAR(100) NULL,
    BankAccountNumber NVARCHAR(50) NULL,
    IFSCode NVARCHAR(50) NULL,
    CreditLimit DECIMAL(18,2) NOT NULL DEFAULT 0,
    OpeningBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    OpeningBalanceType NVARCHAR(10) NOT NULL DEFAULT 'Credit',
    LedgerID INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME NULL,
    CreatedBy NVARCHAR(450) NULL,
    CONSTRAINT FK_Vendors_Ledgers FOREIGN KEY (LedgerID) REFERENCES Ledgers(LedgerID) ON DELETE SET NULL
);
GO

CREATE UNIQUE INDEX IX_Vendors_VendorCode ON Vendors(VendorCode);
GO

-- Customers
CREATE TABLE Customers (
    CustomerID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CustomerCode NVARCHAR(50) NOT NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    ContactPerson NVARCHAR(200) NULL,
    Email NVARCHAR(100) NULL,
    Phone NVARCHAR(20) NULL,
    Mobile NVARCHAR(20) NULL,
    Address NVARCHAR(500) NULL,
    City NVARCHAR(100) NULL,
    State NVARCHAR(50) NULL,
    ZipCode NVARCHAR(20) NULL,
    Country NVARCHAR(50) NULL,
    TaxID NVARCHAR(50) NULL,
    BankName NVARCHAR(100) NULL,
    BankAccountNumber NVARCHAR(50) NULL,
    IFSCode NVARCHAR(50) NULL,
    CreditLimit DECIMAL(18,2) NOT NULL DEFAULT 0,
    OpeningBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    OpeningBalanceType NVARCHAR(10) NOT NULL DEFAULT 'Debit',
    LedgerID INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME NULL,
    CreatedBy NVARCHAR(450) NULL,
    CONSTRAINT FK_Customers_Ledgers FOREIGN KEY (LedgerID) REFERENCES Ledgers(LedgerID) ON DELETE SET NULL
);
GO

CREATE UNIQUE INDEX IX_Customers_CustomerCode ON Customers(CustomerCode);
GO

-- JournalEntries
CREATE TABLE JournalEntries (
    EntryID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EntryNumber NVARCHAR(50) NOT NULL,
    EntryDate DATETIME NOT NULL,
    ReferenceNumber NVARCHAR(100) NULL,
    Description NVARCHAR(500) NULL,
    VoucherType NVARCHAR(50) NOT NULL DEFAULT 'Journal',
    FinancialYearID INT NULL,
    VendorID INT NULL,
    CustomerID INT NULL,
    TotalDebit DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalCredit DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedBy NVARCHAR(450) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    IsPosted BIT NOT NULL DEFAULT 0,
    IsCancelled BIT NOT NULL DEFAULT 0,
    CancellationReason NVARCHAR(500) NULL,
    CONSTRAINT FK_JournalEntries_FinancialYears FOREIGN KEY (FinancialYearID) REFERENCES FinancialYears(FinancialYearID) ON DELETE SET NULL,
    CONSTRAINT FK_JournalEntries_Vendors FOREIGN KEY (VendorID) REFERENCES Vendors(VendorID) ON DELETE SET NULL,
    CONSTRAINT FK_JournalEntries_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID) ON DELETE SET NULL
);
GO

CREATE UNIQUE INDEX IX_JournalEntries_EntryNumber ON JournalEntries(EntryNumber);
GO

-- JournalEntryLines
CREATE TABLE JournalEntryLines (
    LineID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EntryID INT NOT NULL,
    LedgerID INT NOT NULL,
    DebitAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreditAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Description NVARCHAR(500) NULL,
    Sequence INT NULL,
    CONSTRAINT FK_JournalEntryLines_JournalEntries FOREIGN KEY (EntryID) REFERENCES JournalEntries(EntryID) ON DELETE CASCADE,
    CONSTRAINT FK_JournalEntryLines_Ledgers FOREIGN KEY (LedgerID) REFERENCES Ledgers(LedgerID) ON DELETE NO ACTION
);
GO

-- LedgerBalances
CREATE TABLE LedgerBalances (
    BalanceID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    LedgerID INT NOT NULL,
    BalanceDate DATETIME NOT NULL,
    DebitBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreditBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    NetBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    BalanceType NVARCHAR(10) NOT NULL DEFAULT 'Debit',
    CONSTRAINT FK_LedgerBalances_Ledgers FOREIGN KEY (LedgerID) REFERENCES Ledgers(LedgerID) ON DELETE CASCADE
);
GO

-- Schedules
CREATE TABLE Schedules (
    ScheduleID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ScheduleCode NVARCHAR(50) NOT NULL,
    ScheduleName NVARCHAR(200) NOT NULL,
    ScheduleType NVARCHAR(50) NULL,
    Description NVARCHAR(1000) NULL,
    LinkedLedgerID INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Schedules_Ledgers FOREIGN KEY (LinkedLedgerID) REFERENCES Ledgers(LedgerID) ON DELETE SET NULL
);
GO

CREATE UNIQUE INDEX IX_Schedules_ScheduleCode ON Schedules(ScheduleCode);
GO

-- ScheduleItems
CREATE TABLE ScheduleItems (
    ItemID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ScheduleID INT NOT NULL,
    ItemName NVARCHAR(200) NOT NULL,
    ItemCode NVARCHAR(50) NULL,
    Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Remarks NVARCHAR(500) NULL,
    ItemDate DATETIME NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_ScheduleItems_Schedules FOREIGN KEY (ScheduleID) REFERENCES Schedules(ScheduleID) ON DELETE CASCADE
);
GO

-- VoucherNumbers
CREATE TABLE VoucherNumbers (
    VoucherNumberID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    VoucherType NVARCHAR(50) NOT NULL,
    FinancialYearID INT NOT NULL,
    Prefix NVARCHAR(20) NOT NULL,
    CurrentNumber INT NOT NULL DEFAULT 0,
    Suffix NVARCHAR(20) NULL,
    Format NVARCHAR(50) NOT NULL DEFAULT '{Prefix}-{Number:0000}',
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME NULL,
    CONSTRAINT FK_VoucherNumbers_FinancialYears FOREIGN KEY (FinancialYearID) REFERENCES FinancialYears(FinancialYearID) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX IX_VoucherNumbers_VoucherType_FinancialYearID ON VoucherNumbers(VoucherType, FinancialYearID);
GO

-- BankReconciliations
CREATE TABLE BankReconciliations (
    ReconciliationID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    LedgerID INT NOT NULL,
    StatementDate DATETIME NOT NULL,
    BookBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    StatementBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    ReconciledBy NVARCHAR(450) NULL,
    ReconciledDate DATETIME NULL,
    IsReconciled BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_BankReconciliations_Ledgers FOREIGN KEY (LedgerID) REFERENCES Ledgers(LedgerID) ON DELETE NO ACTION
);
GO

-- BankReconciliationItems
CREATE TABLE BankReconciliationItems (
    ItemID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ReconciliationID INT NOT NULL,
    ItemType NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Description NVARCHAR(500) NULL,
    ReferenceNumber NVARCHAR(100) NULL,
    ItemDate DATETIME NULL,
    IsCleared BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_BankReconciliationItems_BankReconciliations FOREIGN KEY (ReconciliationID) REFERENCES BankReconciliations(ReconciliationID) ON DELETE CASCADE
);
GO

-- AuditLogs
CREATE TABLE AuditLogs (
    AuditLogID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EntityType NVARCHAR(50) NOT NULL,
    EntityID INT NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NULL,
    UserID NVARCHAR(450) NULL,
    UserName NVARCHAR(200) NULL,
    IPAddress NVARCHAR(50) NULL,
    ActionDate DATETIME NOT NULL DEFAULT GETDATE(),
    OldValues TEXT NULL,
    NewValues TEXT NULL
);
GO

-- =============================================
-- Create Additional Indexes for Performance
-- =============================================

CREATE INDEX IX_JournalEntries_EntryDate ON JournalEntries(EntryDate);
CREATE INDEX IX_JournalEntries_VoucherType ON JournalEntries(VoucherType);
CREATE INDEX IX_JournalEntries_FinancialYearID ON JournalEntries(FinancialYearID);
CREATE INDEX IX_JournalEntryLines_EntryID ON JournalEntryLines(EntryID);
CREATE INDEX IX_JournalEntryLines_LedgerID ON JournalEntryLines(LedgerID);
CREATE INDEX IX_LedgerBalances_LedgerID ON LedgerBalances(LedgerID);
CREATE INDEX IX_LedgerBalances_BalanceDate ON LedgerBalances(BalanceDate);
CREATE INDEX IX_AuditLogs_EntityType_EntityID ON AuditLogs(EntityType, EntityID);
CREATE INDEX IX_AuditLogs_UserID ON AuditLogs(UserID);
CREATE INDEX IX_AuditLogs_ActionDate ON AuditLogs(ActionDate);
GO

-- =============================================
-- Schema Creation Complete
-- =============================================
PRINT 'Database schema created successfully!';
GO

