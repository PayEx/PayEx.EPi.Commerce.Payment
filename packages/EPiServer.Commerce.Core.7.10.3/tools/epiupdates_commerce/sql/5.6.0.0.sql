--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 5, @minor int = 6, @patch int = 0    
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
 
GO 

CREATE PROCEDURE [dbo].[ecf_CatalogItem_AssetKey]
	@ApplicationId uniqueidentifier,
	@AssetKey nvarchar(254)
AS
BEGIN
	SELECT A.* from [CatalogItemAsset] A
		LEFT OUTER JOIN [CatalogEntry] CE ON CE.CatalogEntryId = A.CatalogEntryId
		LEFT OUTER JOIN [CatalogNode] CN ON CN.CatalogNodeId = A.CatalogNodeId
	WHERE
		(CE.ApplicationId = @ApplicationId OR CN.ApplicationId = @ApplicationId) AND
		A.AssetKey = @AssetKey
END

GO

ALTER TABLE dbo.CatalogItemSeo DROP CONSTRAINT PK_CatalogItemSeo
GO

ALTER TABLE dbo.CatalogItemSeo ADD CONSTRAINT PK_CatalogItemSeo PRIMARY KEY CLUSTERED 
(
	Uri ASC,
	ApplicationId ASC, 
	LanguageCode ASC
)
GO

--make sure lock_escalation is table, it's the default though
ALTER TABLE dbo.CatalogItemSeo SET (LOCK_ESCALATION = TABLE)
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_CatalogEntry_UriLanguage]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_CatalogEntry_UriLanguage]
GO

CREATE PROCEDURE [dbo].[ecf_CatalogEntry_UriLanguage]
	@ApplicationId uniqueidentifier,
	@Uri nvarchar(255),
	@LanguageCode nvarchar(50),
	@ReturnInactive bit = 0
AS
BEGIN
	SELECT TOP(1) N.* from [CatalogEntry] N 
	INNER JOIN CatalogItemSeo S ON N.CatalogEntryId = S.CatalogEntryId
	WHERE
		N.ApplicationId = @ApplicationId AND
		N.ApplicationId = S.ApplicationId  AND
		S.Uri = @Uri AND (S.LanguageCode = @LanguageCode OR @LanguageCode is NULL) AND
		((N.IsActive = 1) or @ReturnInactive = 1)

	SELECT S.* from CatalogItemSeo S
	INNER JOIN CatalogEntry N ON N.CatalogEntryId = S.CatalogEntryId
	WHERE
		S.ApplicationId = @ApplicationId AND
		N.ApplicationId = S.ApplicationId AND
		S.Uri = @Uri AND (S.LanguageCode = @LanguageCode OR @LanguageCode is NULL) AND
		((N.IsActive = 1) or @ReturnInactive = 1)
END
GO



 
--beginUpdatingDatabaseVersion 
 
INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(5, 6, 0, GETUTCDATE()) 
 
GO 

--endUpdatingDatabaseVersion 
