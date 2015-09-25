--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 6, @minor int = 3, @patch int = 0    
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
 
GO 

CREATE TYPE [dbo].[udttContentList] AS TABLE (
    [ContentId] INT NULL); 

GO 

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_GuidMappingCatalog_FindGuids]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_GuidMappingCatalog_FindGuids] 

GO 

CREATE PROCEDURE [dbo].[ecf_GuidMappingCatalog_FindGuids]
	@ContentList udttCatalogList readonly
AS
BEGIN
	select GuidCatalogMapping.CatalogId as Id, ContentGuid
	from dbo.GuidCatalogMapping
	INNER JOIN @ContentList as idTable on idTable.CatalogId = GuidCatalogMapping.CatalogId
END

GO 

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_GuidMappingEntry_FindGuids]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_GuidMappingEntry_FindGuids] 

GO 

CREATE PROCEDURE [dbo].[ecf_GuidMappingEntry_FindGuids]
	@ContentList udttContentList readonly
AS
BEGIN
	select GuidEntryMapping.CatalogEntryId as Id, ContentGuid
	from dbo.GuidEntryMapping
	INNER JOIN @ContentList as idTable on idTable.ContentId = GuidEntryMapping.CatalogEntryId	
END

GO 

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_GuidMappingNode_FindGuids]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_GuidMappingNode_FindGuids] 

GO 

CREATE PROCEDURE [dbo].[ecf_GuidMappingNode_FindGuids]
	@ContentList udttContentList readonly
AS
BEGIN
	select GuidNodeMapping.CatalogNodeId as Id, ContentGuid
	from dbo.GuidNodeMapping
	INNER JOIN @ContentList as idTable on idTable.ContentId = GuidNodeMapping.CatalogNodeId	
END

GO 

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_NodeEntryRelations]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_NodeEntryRelations] 

GO 

CREATE PROCEDURE [dbo].[ecf_NodeEntryRelations]
	@ContentList udttContentList readonly
AS
BEGIN
	Select NodeEntryRelation.CatalogId, CatalogEntryId, CatalogNodeId
	FROM NodeEntryRelation
	INNER JOIN @ContentList as idTable on idTable.ContentId = NodeEntryRelation.CatalogEntryId
	ORDER BY SortOrder
END

GO 

 
--beginUpdatingDatabaseVersion 
 
INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(6, 3, 0, GETUTCDATE()) 
 
GO 

--endUpdatingDatabaseVersion 
