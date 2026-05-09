-- SDSC Payroll System - SQL Server Management Studio (SSMS) Schema Script
-- Created for Entity Framework Core alignment

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SDSC_Payroll')
BEGIN
    CREATE DATABASE [SDSC_Payroll];
END
GO

USE [SDSC_Payroll];
GO

-- 1. Users Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [UserId]       INT IDENTITY(1,1) NOT NULL,
        [Username]     NVARCHAR(50) NOT NULL,
        [PasswordHash] NVARCHAR(255) NOT NULL,
        [FullName]     NVARCHAR(100) NOT NULL,
        [Role]         INT NOT NULL,          -- 0 = Admin, 1 = HR
        [IsActive]     BIT NOT NULL DEFAULT 1,
        [CreatedAt]    DATETIME2 NOT NULL,
        [LastLoginAt]  DATETIME2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserId] ASC)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Users_Username' AND object_id = OBJECT_ID(N'[dbo].[Users]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Username] ON [dbo].[Users] ([Username] ASC);
END
GO

-- 2. Employees Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Employees] (
        [EmployeeId]         INT IDENTITY(1,1) NOT NULL,
        [EmployeeCode]       NVARCHAR(20) NOT NULL,
        [FirstName]          NVARCHAR(100) NOT NULL,
        [LastName]           NVARCHAR(100) NOT NULL,
        [MiddleName]         NVARCHAR(50) NULL,
        [Department]         NVARCHAR(100) NOT NULL,
        [Position]           NVARCHAR(100) NOT NULL,
        [EmployeeType]       INT NOT NULL,           -- 0 = Teaching, 1 = NonTeaching
        [PayrollType]        INT NOT NULL,           -- 0 = Monthly, 1 = HourlyUnit
        [BasicMonthlySalary] DECIMAL(18,2) NOT NULL,
        [HourlyRate]         DECIMAL(18,2) NOT NULL,
        [SSSNumber]          NVARCHAR(20) NULL,
        [TINNumber]          NVARCHAR(20) NULL,
        [PhilHealthNumber]   NVARCHAR(20) NULL,
        [PagIBIGNumber]      NVARCHAR(20) NULL,
        [BiometricId]        NVARCHAR(30) NULL,
        [DateHired]          DATE NOT NULL,
        [IsActive]           BIT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED ([EmployeeId] ASC)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Employees_EmployeeCode' AND object_id = OBJECT_ID(N'[dbo].[Employees]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Employees_EmployeeCode] ON [dbo].[Employees] ([EmployeeCode] ASC);
END
GO

-- 3. AttendanceRecords Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AttendanceRecords]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AttendanceRecords] (
        [AttendanceId]  INT IDENTITY(1,1) NOT NULL,
        [EmployeeId]    INT NOT NULL,
        [Date]          DATE NOT NULL,
        [TimeIn]        TIME NULL,
        [TimeOut]       TIME NULL,
        [LateMinutes]   DECIMAL(7,2) NOT NULL DEFAULT 0,
        [OvertimeHours] DECIMAL(7,2) NOT NULL DEFAULT 0,
        [LeaveType]     INT NOT NULL DEFAULT 0, -- 0=None, 1=Sick, 2=Vacation
        [IsAbsent]      BIT NOT NULL DEFAULT 0,
        [Remarks]       NVARCHAR(200) NULL,
        CONSTRAINT [PK_AttendanceRecords] PRIMARY KEY CLUSTERED ([AttendanceId] ASC),
        CONSTRAINT [FK_AttendanceRecords_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([EmployeeId]) ON DELETE NO ACTION
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_AttendanceRecords_EmployeeId_Date' AND object_id = OBJECT_ID(N'[dbo].[AttendanceRecords]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_AttendanceRecords_EmployeeId_Date] ON [dbo].[AttendanceRecords] ([EmployeeId] ASC, [Date] ASC);
END
GO

-- 4. PayrollRecords Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PayrollRecords]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PayrollRecords] (
        [PayrollId]           INT IDENTITY(1,1) NOT NULL,
        [EmployeeId]          INT NOT NULL,
        [PayYear]             INT NOT NULL,
        [PayMonth]            INT NOT NULL,
        [PayPeriod]           INT NOT NULL, -- 0=FirstHalf, 1=SecondHalf
        [PayPeriodStart]      DATE NOT NULL,
        [PayPeriodEnd]        DATE NOT NULL,
        [BasicPay]            DECIMAL(18,2) NOT NULL,
        [OvertimePay]         DECIMAL(18,2) NOT NULL,
        [GrossPay]            DECIMAL(18,2) NOT NULL,
        [TardinessDeduction]  DECIMAL(18,2) NOT NULL,
        [AbsenceDeduction]    DECIMAL(18,2) NOT NULL,
        [SSS_Employee]        DECIMAL(18,2) NOT NULL,
        [PhilHealth_Employee] DECIMAL(18,2) NOT NULL,
        [PagIBIG_Employee]    DECIMAL(18,2) NOT NULL,
        [SSS_Employer]        DECIMAL(18,2) NOT NULL,
        [PhilHealth_Employer] DECIMAL(18,2) NOT NULL,
        [PagIBIG_Employer]    DECIMAL(18,2) NOT NULL,
        [TaxableIncome]       DECIMAL(18,2) NOT NULL,
        [WithholdingTax]      DECIMAL(18,2) NOT NULL,
        [LoanDeductions]      DECIMAL(18,2) NOT NULL,
        [NetPay]              DECIMAL(18,2) NOT NULL,
        [IsPosted]            BIT NOT NULL DEFAULT 0,
        [Is13thMonth]         BIT NOT NULL DEFAULT 0,
        [GeneratedDate]       DATETIME2 NOT NULL,
        [Remarks]             NVARCHAR(500) NULL,
        CONSTRAINT [PK_PayrollRecords] PRIMARY KEY CLUSTERED ([PayrollId] ASC),
        CONSTRAINT [FK_PayrollRecords_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([EmployeeId]) ON DELETE NO ACTION
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_PayrollRecords_CompositeKey' AND object_id = OBJECT_ID(N'[dbo].[PayrollRecords]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_PayrollRecords_CompositeKey] ON [dbo].[PayrollRecords] ([EmployeeId] ASC, [PayYear] ASC, [PayMonth] ASC, [PayPeriod] ASC, [Is13thMonth] ASC);
END
GO

-- 5. LoanRecords Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoanRecords]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LoanRecords] (
        [LoanId]              INT IDENTITY(1,1) NOT NULL,
        [EmployeeId]          INT NOT NULL,
        [LoanType]            INT NOT NULL, -- 0=SSS, 1=PagIBIG, 2=CashAdvance
        [LoanAmount]          DECIMAL(18,2) NOT NULL,
        [Balance]             DECIMAL(18,2) NOT NULL,
        [MonthlyAmortization] DECIMAL(18,2) NOT NULL,
        [StartDate]           DATE NOT NULL,
        [EndDate]             DATE NULL,
        [Status]              INT NOT NULL, -- 0=Active, 1=FullyPaid, 2=Cancelled
        [Remarks]             NVARCHAR(500) NULL,
        [CreatedAt]           DATETIME2 NOT NULL,
        CONSTRAINT [PK_LoanRecords] PRIMARY KEY CLUSTERED ([LoanId] ASC),
        CONSTRAINT [FK_LoanRecords_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([EmployeeId]) ON DELETE NO ACTION
    );
END
GO

-- 6. LoanPayments Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoanPayments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LoanPayments] (
        [LoanPaymentId] INT IDENTITY(1,1) NOT NULL,
        [LoanId]        INT NOT NULL,
        [PayrollId]     INT NULL,  -- null if manual payment
        [AmountPaid]    DECIMAL(18,2) NOT NULL,
        [PaymentDate]   DATE NOT NULL,
        [BalanceAfter]  DECIMAL(18,2) NOT NULL,
        [Remarks]       NVARCHAR(200) NULL,
        [CreatedAt]     DATETIME2 NOT NULL,
        CONSTRAINT [PK_LoanPayments] PRIMARY KEY CLUSTERED ([LoanPaymentId] ASC),
        CONSTRAINT [FK_LoanPayments_LoanRecords_LoanId] FOREIGN KEY ([LoanId]) REFERENCES [dbo].[LoanRecords] ([LoanId]) ON DELETE CASCADE
    );
END
GO

-- 7. SystemLogs Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SystemLogs] (
        [LogId]     INT IDENTITY(1,1) NOT NULL,
        [Timestamp] DATETIME2 NOT NULL,
        [Username]  NVARCHAR(100) NOT NULL,
        [Module]    NVARCHAR(50)  NOT NULL,
        [Action]    NVARCHAR(250) NOT NULL,
        [Details]   NVARCHAR(1000) NULL,
        [Severity]  INT NOT NULL DEFAULT 0,  -- 0=Info, 1=Warning, 2=Error
        CONSTRAINT [PK_SystemLogs] PRIMARY KEY CLUSTERED ([LogId] ASC)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_SystemLogs_Timestamp' AND object_id = OBJECT_ID(N'[dbo].[SystemLogs]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SystemLogs_Timestamp] ON [dbo].[SystemLogs] ([Timestamp] DESC);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_SystemLogs_Severity' AND object_id = OBJECT_ID(N'[dbo].[SystemLogs]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SystemLogs_Severity] ON [dbo].[SystemLogs] ([Severity] ASC);
END
GO

-- Insert default admin if not exists (using BCrypt hash for "Admin@1234")
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Username] = 'admin')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [PasswordHash], [FullName], [Role], [IsActive], [CreatedAt])
    VALUES ('admin', '$2a$11$jE1E.yGv6pUSuA0r1p2f6OV.hN8fLIf60mStfA1z2B9mYt1GvKkO2', 'System Administrator', 0, 1, '2026-01-01');
END
GO
