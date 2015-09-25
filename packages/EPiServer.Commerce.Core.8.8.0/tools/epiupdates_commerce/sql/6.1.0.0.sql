--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 6, @minor int = 1, @patch int = 0
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
GO

ALTER TABLE [dbo].[Variation] ADD Length FLOAT (53) NOT NULL DEFAULT(0)
ALTER TABLE [dbo].[Variation] ADD Height FLOAT (53) NOT NULL DEFAULT(0)
ALTER TABLE [dbo].[Variation] ADD Width FLOAT (53) NOT NULL DEFAULT(0)
GO

ALTER TABLE [dbo].[Catalog] ADD LengthBase NVARCHAR(128) NULL
GO

--beginUpdatingDatabaseVersion 
 
INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(6, 1, 0, GETUTCDATE()) 
GO 

--endUpdatingDatabaseVersion 