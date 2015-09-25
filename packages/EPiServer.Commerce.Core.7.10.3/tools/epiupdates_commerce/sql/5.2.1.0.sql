--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 5, @minor int = 2, @patch int = 1    
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
 
GO 

IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('dbo.CatalogItemSeo') AND NAME ='IX_CatalogItemSeo_UniqueSegment_CatalogEntry')
    DROP INDEX IX_CatalogItemSeo_UniqueSegment_CatalogEntry ON dbo.CatalogItemSeo;
GO

CREATE NONCLUSTERED INDEX [IX_CatalogItemSeo_UniqueSegment_CatalogEntry] ON [dbo].[CatalogItemSeo]
(
    [ApplicationId] ASC,
    [UriSegment] ASC,
    [CatalogEntryId] ASC
)
GO
 
--beginUpdatingDatabaseVersion 
 
INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(5, 2, 1, GETUTCDATE()) 
 
GO 

--endUpdatingDatabaseVersion 
