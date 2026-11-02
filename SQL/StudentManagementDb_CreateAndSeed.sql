/* =========================================================================================
   Student Management System - Database Creation & Seed Script
   Target: Microsoft SQL Server 2019+ (also works on SQL Server 2022 / Azure SQL)
   -----------------------------------------------------------------------------------------
   This script is idempotent-ish: it drops and recreates the database so it can be re-run
   safely during development/teaching. DO NOT run the DROP DATABASE section in production.
   ========================================================================================= */

USE master;
GO

-- -----------------------------------------------------------------------------------------
-- 1. CREATE DATABASE
-- -----------------------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = N'StudentManagementDb')
BEGIN
    ALTER DATABASE StudentManagementDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE StudentManagementDb;
END
GO

CREATE DATABASE StudentManagementDb;
GO

USE StudentManagementDb;
GO

-- -----------------------------------------------------------------------------------------
-- 2. CREATE TABLE: Students
--    - Primary Key:      StudentId (identity)
--    - Unique:           StudentNumber, Email
--    - NOT NULL:         all core columns
--    - Default values:   DateCreated, IsActive
-- -----------------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Students', 'U') IS NOT NULL
    DROP TABLE dbo.Students;
GO

CREATE TABLE dbo.Students
(
    StudentId       INT              IDENTITY(1,1) NOT NULL,
    StudentNumber   NVARCHAR(20)     NOT NULL,
    FirstName       NVARCHAR(100)    NOT NULL,
    LastName        NVARCHAR(100)    NOT NULL,
    Email           NVARCHAR(150)    NOT NULL,
    Course          NVARCHAR(100)    NOT NULL,
    YearLevel       INT              NOT NULL,
    DateCreated     DATETIME2        NOT NULL CONSTRAINT DF_Students_DateCreated DEFAULT (GETDATE()),
    DateUpdated     DATETIME2        NULL,
    IsActive        BIT              NOT NULL CONSTRAINT DF_Students_IsActive DEFAULT (1),

    CONSTRAINT PK_Students PRIMARY KEY CLUSTERED (StudentId ASC),

    -- CHECK constraint: enforce a realistic year level range at the database layer too,
    -- mirroring the [Range(1,6)] Data Annotation on the C# model.
    CONSTRAINT CK_Students_YearLevel CHECK (YearLevel BETWEEN 1 AND 6),

    -- CHECK constraint: very basic sanity check that Email contains an "@" symbol.
    CONSTRAINT CK_Students_Email_Format CHECK (Email LIKE '%_@__%.__%')
);
GO

-- -----------------------------------------------------------------------------------------
-- 3. UNIQUE CONSTRAINTS / INDEXES
-- -----------------------------------------------------------------------------------------

-- Unique constraint on StudentNumber (no two students can share the same student number).
ALTER TABLE dbo.Students
    ADD CONSTRAINT UQ_Students_StudentNumber UNIQUE (StudentNumber);
GO

-- Unique constraint on Email (no two students can share the same email address).
ALTER TABLE dbo.Students
    ADD CONSTRAINT UQ_Students_Email UNIQUE (Email);
GO

-- Non-unique composite index to speed up name-based searching/sorting on the list page.
CREATE NONCLUSTERED INDEX IX_Students_LastName_FirstName
    ON dbo.Students (LastName ASC, FirstName ASC);
GO

-- Index to speed up filtering/grouping by Course (used on the Dashboard and search).
CREATE NONCLUSTERED INDEX IX_Students_Course
    ON dbo.Students (Course ASC);
GO

-- Filtered index: most queries only care about active (non soft-deleted) students,
-- so a filtered index keeps it small and fast.
CREATE NONCLUSTERED INDEX IX_Students_IsActive
    ON dbo.Students (IsActive)
    WHERE IsActive = 1;
GO

-- -----------------------------------------------------------------------------------------
-- 4. SEED DATA - at least 20 sample records
-- -----------------------------------------------------------------------------------------
INSERT INTO dbo.Students (StudentNumber, FirstName, LastName, Email, Course, YearLevel, DateCreated, IsActive)
VALUES
    (N'2026-00001', N'Juan',     N'Dela Cruz',   N'juan.delacruz@example.edu',    N'BS Computer Science',        1, GETDATE(), 1),
    (N'2026-00002', N'Maria',    N'Santos',      N'maria.santos@example.edu',     N'BS Information Technology',  2, GETDATE(), 1),
    (N'2026-00003', N'Jose',     N'Reyes',       N'jose.reyes@example.edu',       N'BS Computer Science',        3, GETDATE(), 1),
    (N'2026-00004', N'Ana',      N'Garcia',      N'ana.garcia@example.edu',       N'BS Accountancy',             1, GETDATE(), 1),
    (N'2026-00005', N'Pedro',    N'Bautista',    N'pedro.bautista@example.edu',   N'BS Civil Engineering',       4, GETDATE(), 1),
    (N'2026-00006', N'Sofia',    N'Cruz',        N'sofia.cruz@example.edu',       N'BS Nursing',                 2, GETDATE(), 1),
    (N'2026-00007', N'Miguel',   N'Torres',      N'miguel.torres@example.edu',    N'BS Information Technology',  3, GETDATE(), 1),
    (N'2026-00008', N'Isabella', N'Flores',      N'isabella.flores@example.edu',  N'BS Psychology',              1, GETDATE(), 1),
    (N'2026-00009', N'Gabriel',  N'Ramos',       N'gabriel.ramos@example.edu',    N'BS Computer Science',        2, GETDATE(), 1),
    (N'2026-00010', N'Camila',   N'Mendoza',     N'camila.mendoza@example.edu',   N'BS Business Administration', 4, GETDATE(), 1),
    (N'2026-00011', N'Diego',    N'Aquino',      N'diego.aquino@example.edu',     N'BS Electrical Engineering',  3, GETDATE(), 1),
    (N'2026-00012', N'Valentina',N'Castillo',    N'valentina.castillo@example.edu', N'BS Nursing',               1, GETDATE(), 1),
    (N'2026-00013', N'Lucas',    N'Villanueva',  N'lucas.villanueva@example.edu', N'BS Information Technology',  2, GETDATE(), 1),
    (N'2026-00014', N'Emma',     N'Navarro',     N'emma.navarro@example.edu',     N'BS Accountancy',             3, GETDATE(), 1),
    (N'2026-00015', N'Mateo',    N'Salazar',     N'mateo.salazar@example.edu',    N'BS Computer Science',        4, GETDATE(), 1),
    (N'2026-00016', N'Mia',      N'Gonzales',    N'mia.gonzales@example.edu',     N'BS Psychology',              2, GETDATE(), 1),
    (N'2026-00017', N'Daniel',   N'Pascual',     N'daniel.pascual@example.edu',   N'BS Civil Engineering',       1, GETDATE(), 1),
    (N'2026-00018', N'Lucia',    N'Marquez',     N'lucia.marquez@example.edu',    N'BS Business Administration', 3, GETDATE(), 1),
    (N'2026-00019', N'Rafael',   N'Domingo',     N'rafael.domingo@example.edu',   N'BS Electrical Engineering',  2, GETDATE(), 1),
    (N'2026-00020', N'Elena',    N'Ocampo',      N'elena.ocampo@example.edu',     N'BS Nursing',                 4, GETDATE(), 1),
    (N'2026-00021', N'Adrian',   N'Villareal',   N'adrian.villareal@example.edu', N'BS Information Technology',  1, GETDATE(), 1),
    (N'2026-00022', N'Carmela',  N'Del Rosario', N'carmela.delrosario@example.edu', N'BS Computer Science',      2, GETDATE(), 1);
GO

-- -----------------------------------------------------------------------------------------
-- 5. VERIFICATION QUERIES (optional - run manually to sanity-check the seed)
-- -----------------------------------------------------------------------------------------
-- SELECT COUNT(*) AS TotalStudents FROM dbo.Students;
-- SELECT Course, COUNT(*) AS StudentsPerCourse FROM dbo.Students GROUP BY Course ORDER BY Course;
-- SELECT * FROM dbo.Students ORDER BY LastName;

PRINT 'StudentManagementDb created and seeded successfully.';
GO
