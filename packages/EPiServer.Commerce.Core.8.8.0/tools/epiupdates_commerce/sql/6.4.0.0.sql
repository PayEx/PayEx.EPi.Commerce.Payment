--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 6, @minor int = 4, @patch int = 0
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
 
GO 

ALTER TABLE dbo.Shipment ADD OperationKeys NVARCHAR (MAX) NULL
GO

-- create OperationKeys meta field
IF NOT EXISTS(SELECT * FROM [dbo].[MetaField] 
        WHERE [Name] = N'OperationKeys')
	AND EXISTS(SELECT TOP 1 MetaClassId from MetaClass WHERE Name = 'Shipment')
BEGIN
	DECLARE @metaClassId int, @metaDataTypeId int, @metaFieldId int
	SET @metaClassId = (SELECT TOP 1 MetaClassId from MetaClass WHERE Name = 'Shipment')
	SET @metaDataTypeId = (SELECT TOP 1 DataTypeId from MetaDataType WHERE Name = 'NVarChar')
	
	-- create OperationKeys meta field
	INSERT INTO [dbo].[MetaField]
           ([Name]
           ,[Namespace]
           ,[SystemMetaClassId]
           ,[FriendlyName]
           ,[Description]
           ,[DataTypeId]
           ,[Length]
           ,[AllowNulls]
           ,[MultiLanguageValue]
           ,[AllowSearch]
           ,[IsEncrypted]
           ,[IsKeyField])
	VALUES
           (
		   'OperationKeys'
           ,'Mediachase.Commerce.Orders.System.Shipment'
           ,@metaClassId
           ,'OperationKeys'
           ,'Operation keys for inventory requests'
           ,@metaDataTypeId
           ,-1
           ,1
           ,0
           ,0
           ,0
           ,0)
		   
	SET @metaFieldId = (SELECT TOP 1 MetaFieldId from MetaField WHERE Name = 'OperationKeys')
		   
	-- add relation between Shipment and OperationKeys
	INSERT INTO [dbo].[MetaClassMetaFieldRelation] ([MetaClassId], [MetaFieldId])
	VALUES (@metaClassId, @metaFieldId)

	SET @metaClassId = (SELECT TOP 1 MetaClassId from MetaClass WHERE Name = 'ShipmentEx')

	-- add relation between ShipmentEx and OperationKeys
	INSERT INTO [dbo].[MetaClassMetaFieldRelation] ([MetaClassId], [MetaFieldId])
	VALUES (@metaClassId, @metaFieldId)

	END
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Shipment_Insert]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Shipment_Insert] 

GO 

CREATE PROCEDURE [dbo].[ecf_Shipment_Insert]
(
	@ShipmentId int = NULL OUTPUT,
	@OrderFormId int,
	@OrderGroupId int,
	@ShippingMethodId uniqueidentifier,
	@ShippingAddressId nvarchar(50) = NULL,
	@ShipmentTrackingNumber nvarchar(128) = NULL,
	@ShipmentTotal money,
	@ShippingDiscountAmount money,
	@ShippingMethodName nvarchar(128) = NULL,
	@ShippingTax money,
	@Status nvarchar(64) = NULL,
	@LineItemIds nvarchar(max) = NULL,
	@WarehouseCode nvarchar(50) = NULL,
	@PickListId int = NULL,
	@SubTotal money,
	@OperationKeys nvarchar(max) = NULL
)
AS
	SET NOCOUNT ON

	INSERT INTO [Shipment]
	(
		[OrderFormId],
		[OrderGroupId],
		[ShippingMethodId],
		[ShippingAddressId],
		[ShipmentTrackingNumber],
		[ShipmentTotal],
		[ShippingDiscountAmount],
		[ShippingMethodName],
		[ShippingTax],
		[Status],
		[LineItemIds],
		[WarehouseCode],
		[PickListId],
		[SubTotal],
		[OperationKeys]
	)
	VALUES
	(
		@OrderFormId,
		@OrderGroupId,
		@ShippingMethodId,
		@ShippingAddressId,
		@ShipmentTrackingNumber,
		@ShipmentTotal,
		@ShippingDiscountAmount,
		@ShippingMethodName,
		@ShippingTax,
		@Status,
		@LineItemIds,
		@WarehouseCode,
		@PickListId,
		@SubTotal,
		@OperationKeys
	)

	SELECT @ShipmentId = SCOPE_IDENTITY()

	RETURN @@Error

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Shipment_Update]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Shipment_Update] 

GO 

CREATE PROCEDURE [dbo].[ecf_Shipment_Update]
(
	@ShipmentId int,
	@OrderFormId int,
	@OrderGroupId int,
	@ShippingMethodId uniqueidentifier,
	@ShippingAddressId nvarchar(50) = NULL,
	@ShipmentTrackingNumber nvarchar(128) = NULL,
	@ShipmentTotal money,
	@ShippingDiscountAmount money,
	@ShippingMethodName nvarchar(128) = NULL,
	@ShippingTax money,
	@Status nvarchar(64) = NULL,
	@LineItemIds nvarchar(max) = NULL,
	@WarehouseCode nvarchar(50) = NULL,
	@PickListId int = NULL,
	@SubTotal money,
	@OperationKeys nvarchar(max) = NULL
)
AS
	SET NOCOUNT ON
	
	UPDATE [Shipment]
	SET
		[OrderFormId] = @OrderFormId,
		[OrderGroupId] = @OrderGroupId,
		[ShippingMethodId] = @ShippingMethodId,
		[ShippingAddressId] = @ShippingAddressId,
		[ShipmentTrackingNumber] = @ShipmentTrackingNumber,
		[ShipmentTotal] = @ShipmentTotal,
		[ShippingDiscountAmount] = @ShippingDiscountAmount,
		[ShippingMethodName] = @ShippingMethodName,
		[ShippingTax] = @ShippingTax,
		[Status] = @Status,
		[LineItemIds] = @LineItemIds,
		[WarehouseCode] = @WarehouseCode,
		[PickListId] = @PickListId,
		[SubTotal] = @SubTotal,
		[OperationKeys] = @OperationKeys
	WHERE 
		[ShipmentId] = @ShipmentId

	RETURN @@Error
GO

-- Setting InventoryService PurchaseAvailable as Variation StartDate when PurchaseAvailable is default date.
UPDATE i SET i.PurchaseAvailableUtc = e.StartDate
FROM [dbo].[InventoryService] i INNER JOIN [dbo].[CatalogEntry] e ON e.Code = i.CatalogEntryCode
WHERE i.PurchaseAvailableUtc = '1900-01-01'
GO

--beginUpdatingDatabaseVersion 
 
INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(6, 4, 0, GETUTCDATE()) 
 
GO 

--endUpdatingDatabaseVersion 
