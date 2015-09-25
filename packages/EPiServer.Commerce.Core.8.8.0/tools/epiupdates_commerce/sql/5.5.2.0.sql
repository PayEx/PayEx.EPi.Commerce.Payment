--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 5, @minor int = 5, @patch int = 2    
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
 
GO 

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_GuidMappingNode_Insert]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_GuidMappingNode_Insert]

GO

CREATE PROCEDURE [dbo].[ecf_GuidMappingNode_Insert]
	@ContentGuid uniqueidentifier,
	@CatalogNodeId int
AS
BEGIN
	IF EXISTS(SELECT(1) FROM CatalogNode WHERE CatalogNodeId = @CatalogNodeId)
	BEGIN
		DELETE 
		FROM dbo.GuidNodeMapping 
		WHERE CatalogNodeId = @CatalogNodeId
	
		INSERT INTO dbo.GuidNodeMapping (ContentGuid, CatalogNodeId)
		VALUES (@ContentGuid, @CatalogNodeId)	
	END
END

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_GuidMappingEntry_Insert]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_GuidMappingEntry_Insert]

GO

CREATE PROCEDURE [dbo].[ecf_GuidMappingEntry_Insert]
	@ContentGuid uniqueidentifier,
	@CatalogEntryId int
AS
BEGIN
	IF EXISTS(SELECT(1) FROM CatalogEntry WHERE CatalogEntryId = @CatalogEntryId)
	BEGIN
		DELETE 
		FROM dbo.GuidEntryMapping 
		WHERE CatalogEntryId = @CatalogEntryId 
	
		INSERT INTO dbo.GuidEntryMapping (ContentGuid, CatalogEntryId)
		VALUES (@ContentGuid, @CatalogEntryId)	
	END
END

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_GuidMappingCatalog_Insert]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_GuidMappingCatalog_Insert]

GO

CREATE PROCEDURE [dbo].[ecf_GuidMappingCatalog_Insert]
	@ContentGuid uniqueidentifier,
	@CatalogId int
AS
BEGIN
	IF EXISTS(SELECT(1) FROM Catalog WHERE CatalogId = @CatalogId)
	BEGIN
		DELETE 
		FROM dbo.GuidCatalogMapping 
		WHERE CatalogId = @CatalogId 
	
		INSERT INTO dbo.GuidCatalogMapping (ContentGuid, CatalogId)
		VALUES (@ContentGuid, @CatalogId)	
	END
END

GO
 
--beginUpdatingDatabaseVersion 
 
INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(5, 5, 2, GETUTCDATE()) 
 
GO 

--endUpdatingDatabaseVersion 
