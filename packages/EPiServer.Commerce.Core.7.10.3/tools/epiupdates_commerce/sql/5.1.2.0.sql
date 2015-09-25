--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 5, @minor int = 1, @patch int = 2    
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
 
GO

ALTER PROCEDURE [dbo].[ecf_WarehouseInventory_GetInventories]
	@ApplicationId UNIQUEIDENTIFIER,
	@CatalogKeys udttCatalogKey READONLY,
	@WarehouseCodes udttWarehouseCode READONLY
AS
BEGIN

    DECLARE
		@filterCatalogKeys BIT = CASE WHEN EXISTS (SELECT 1 FROM @CatalogKeys) THEN 1 ELSE 0 END,
		@filterWarehouseCodes BIT = CASE WHEN EXISTS (SELECT 1 FROM @WarehouseCodes) THEN 1 ELSE 0 END

	SELECT 
		WI.WarehouseCode,
		WI.CatalogEntryCode,
		WI.InStockQuantity,
		WI.ReservedQuantity,
		WI.ReorderMinQuantity,
		WI.PreorderQuantity,
		WI.BackorderQuantity,
		WI.AllowPreorder,
		WI.AllowBackorder,
		WI.InventoryStatus,
		WI.PreorderAvailabilityDate,
		WI.BackorderAvailabilityDate,
		WI.ApplicationId
	FROM [WarehouseInventory] AS WI
	JOIN [Warehouse] AS W ON WI.ApplicationId = W.ApplicationId 
							AND	WI.WarehouseCode = W.Code
        LEFT JOIN @WarehouseCodes as WC ON WI.WarehouseCode = WC.WarehouseCode
		LEFT JOIN @CatalogKeys as CK ON WI.CatalogEntryCode = CK.CatalogEntryCode
										AND	WI.ApplicationId = CK.ApplicationId 
	WHERE WI.ApplicationId = @ApplicationId
	AND (@filterWarehouseCodes = 0 OR WC.WarehouseCode is not NULL)
	AND (@filterCatalogKeys = 0 OR CK.CatalogEntryCode is not NULL)
	ORDER BY W.SortOrder, WI.CatalogEntryCode
END

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_AllCatalogEntry_CatalogId]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_AllCatalogEntry_CatalogId]
GO

CREATE PROCEDURE [dbo].[ecf_AllCatalogEntry_CatalogId]
    @CatalogId int
AS
BEGIN
	
	SELECT N.* from [CatalogEntry] N
	WHERE
		N.CatalogId = @CatalogId 

END

GO

--beginUpdatingDatabaseVersion 
 
INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(5, 1, 2, GETUTCDATE()) 
 
GO 

--endUpdatingDatabaseVersion  
