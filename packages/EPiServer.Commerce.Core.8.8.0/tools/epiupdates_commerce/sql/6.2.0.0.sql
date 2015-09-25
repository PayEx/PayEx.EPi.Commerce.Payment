--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 6, @minor int = 2, @patch int = 0    
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
GO 

-- add ProviderTransactionID column to OrderFormPayment if it does not exist
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'ProviderTransactionID' AND [object_id] = OBJECT_ID(N'OrderFormPayment'))
BEGIN
	ALTER TABLE [dbo].[OrderFormPayment] ADD [ProviderTransactionID] [nvarchar](255) NULL
END
GO

ALTER PROCEDURE [dbo].[ecf_OrderFormPayment_Insert]
(
	@PaymentId int = NULL OUTPUT,
	@OrderFormId int,
	@OrderGroupId int,
	@BillingAddressId nvarchar(50) = NULL,
	@PaymentMethodId uniqueidentifier,
	@PaymentMethodName nvarchar(128) = NULL,
	@CustomerName nvarchar(64) = NULL,
	@Amount money,
	@PaymentType int,
	@ValidationCode nvarchar(64) = NULL,
	@AuthorizationCode nvarchar(255) = NULL,
	@TransactionType nvarchar(255) = NULL,
	@TransactionID nvarchar(255) = NULL,
	@ProviderTransactionID nvarchar(255) = NULL,
	@Status nvarchar(64) = NULL,
	@ImplementationClass nvarchar(255)
)
AS
	SET NOCOUNT ON

	INSERT INTO [OrderFormPayment]
	(
		[OrderFormId],
		[OrderGroupId],
		[BillingAddressId],
		[PaymentMethodId],
		[PaymentMethodName],
		[CustomerName],
		[Amount],
		[PaymentType],
		[ValidationCode],
		[AuthorizationCode],
		[TransactionType],
		[TransactionID],
		[Status],
		[ImplementationClass],
		[ProviderTransactionID]
	)
	VALUES
	(
		@OrderFormId,
		@OrderGroupId,
		@BillingAddressId,
		@PaymentMethodId,
		@PaymentMethodName,
		@CustomerName,
		@Amount,
		@PaymentType,
		@ValidationCode,
		@AuthorizationCode,
		@TransactionType,
		@TransactionID,
		@Status,
		@ImplementationClass,
		@ProviderTransactionID
	)

	SELECT @PaymentId = SCOPE_IDENTITY()

	RETURN @@Error
GO

ALTER PROCEDURE [dbo].[ecf_OrderFormPayment_Update]
(
	@PaymentId int,
	@OrderFormId int,
	@OrderGroupId int,
	@BillingAddressId nvarchar(50) = NULL,
	@PaymentMethodId uniqueidentifier,
	@PaymentMethodName nvarchar(128) = NULL,
	@CustomerName nvarchar(64) = NULL,
	@Amount money,
	@PaymentType int,
	@ValidationCode nvarchar(64) = NULL,
	@AuthorizationCode nvarchar(255) = NULL,
	@TransactionType nvarchar(255) = NULL,
	@TransactionID nvarchar(255) = NULL,
	@ProviderTransactionID nvarchar(255) = NULL,
	@Status nvarchar(64) = NULL,
	@ImplementationClass nvarchar(255)
)
AS
	SET NOCOUNT ON
	
	UPDATE [OrderFormPayment]
	SET
		[OrderFormId] = @OrderFormId,
		[OrderGroupId] = @OrderGroupId,
		[BillingAddressId] = @BillingAddressId,
		[PaymentMethodId] = @PaymentMethodId,
		[PaymentMethodName] = @PaymentMethodName,
		[CustomerName] = @CustomerName,
		[Amount] = @Amount,
		[PaymentType] = @PaymentType,
		[ValidationCode] = @ValidationCode,
		[AuthorizationCode] = @AuthorizationCode,
		[TransactionType] = @TransactionType,
		[TransactionID] = @TransactionID,
		[ProviderTransactionID] = @ProviderTransactionID,
		[Status] = @Status,
		[ImplementationClass] = @ImplementationClass
	WHERE 
		[PaymentId] = @PaymentId

	RETURN @@Error
GO

-- create ProviderTransactionID meta field
IF NOT EXISTS(SELECT * FROM [dbo].[MetaField] 
        WHERE [Name] = N'ProviderTransactionID')
	AND EXISTS(SELECT TOP 1 MetaClassId from MetaClass WHERE Name = 'OrderFormPayment')
BEGIN
	DECLARE @metaClassId int, @metaDataTypeId int, @metaFieldId int
	SET @metaClassId = (SELECT TOP 1 MetaClassId from MetaClass WHERE Name = 'OrderFormPayment')
	SET @metaDataTypeId = (SELECT TOP 1 DataTypeId from MetaDataType WHERE Name = 'NVarChar')
	
	-- create ProviderTransactionID meta field
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
		   'ProviderTransactionID'
           ,'Mediachase.Commerce.Orders.System.OrderFormPayment'
           ,@metaClassId
           ,'Provider Transaction Id'
           ,'The transaction ID which is returned from Payment Provider'
           ,@metaDataTypeId
           ,510
           ,1
           ,0
           ,0
           ,0
           ,0)
		   
	SET @metaFieldId = (SELECT TOP 1 MetaFieldId from MetaField WHERE Name = 'ProviderTransactionID')
		   
	-- add relation between OrderFormPayment and ProviderTransactionID
	INSERT INTO [dbo].[MetaClassMetaFieldRelation] ([MetaClassId], [MetaFieldId])
	VALUES (@metaClassId, @metaFieldId)
			   
	-- add relation between ProviderTransactionID and OrderFormPayment's children
	INSERT INTO [dbo].[MetaClassMetaFieldRelation] ([MetaClassId], [MetaFieldId])
	SELECT MC.MetaClassId, MF.MetaFieldId FROM MetaField MF, MetaClass MC
	WHERE MF.[SystemMetaClassId] = @metaClassId AND MF.MetaFieldId = @metaFieldId AND MC.ParentClassId = @metaClassId
	
END
GO

-- create ProviderProfileId meta fields
IF NOT EXISTS(SELECT * FROM [dbo].[MetaField] 
        WHERE [Name] = N'ProviderProfileId')
	AND EXISTS(SELECT TOP 1 MetaClassId from MetaClass WHERE Name = 'CreditCardPayment')
BEGIN
	DECLARE @metaClassId int, @metaDataTypeId int, @metaFieldId int, @Retval int
	SET @metaClassId = (SELECT TOP 1 MetaClassId from MetaClass WHERE Name = 'CreditCardPayment')
	SET @metaDataTypeId = (SELECT TOP 1 DataTypeId from MetaDataType WHERE Name = 'ShortString')
	
	-- create ProviderProfileId meta field
	EXECUTE [mdpsp_sys_AddMetaField] 
	   'Mediachase.Commerce.Orders.System'
	  ,'ProviderProfileId'
	  ,'Provider customer profile Id'
	  ,'The customer profile ID which is returned from Payment Provider'
	  ,@metaDataTypeId
	  ,512
	  ,1
	  ,0
	  ,0
	  ,0
	  ,@Retval OUTPUT

	SET @metaFieldId = (SELECT TOP 1 MetaFieldId from MetaField WHERE Name = 'ProviderProfileId')
	  
	EXECUTE [mdpsp_sys_AddMetaFieldToMetaClass] 
	   @metaClassId
	  ,@metaFieldId
	  ,0
END
GO

-- create ProviderPaymentId meta fields
IF NOT EXISTS(SELECT * FROM [dbo].[MetaField] 
        WHERE [Name] = N'ProviderPaymentId')
	AND EXISTS(SELECT TOP 1 MetaClassId from MetaClass WHERE Name = 'CreditCardPayment')
BEGIN
	DECLARE @metaClassId int, @metaDataTypeId int, @metaFieldId int, @Retval int
	SET @metaClassId = (SELECT TOP 1 MetaClassId from MetaClass WHERE Name = 'CreditCardPayment')
	SET @metaDataTypeId = (SELECT TOP 1 DataTypeId from MetaDataType WHERE Name = 'ShortString')
	
	-- create ProviderPaymentId meta field
	EXECUTE [mdpsp_sys_AddMetaField] 
	   'Mediachase.Commerce.Orders.System'
	  ,'ProviderPaymentId'
	  ,'Provider payment Id'
	  ,'The payment ID which is returned from Payment Provider'
	  ,@metaDataTypeId
	  ,512
	  ,1
	  ,0
	  ,0
	  ,0
	  ,@Retval OUTPUT

	SET @metaFieldId = (SELECT TOP 1 MetaFieldId from MetaField WHERE Name = 'ProviderPaymentId')
	  
	EXECUTE [mdpsp_sys_AddMetaFieldToMetaClass] 
	   @metaClassId
	  ,@metaFieldId
	  ,0
END
GO

-- add ShippingTax column to Shipment if it does not exist
IF NOT EXISTS(SELECT * FROM sys.columns  
		WHERE [name] = N'ShippingTax' AND [object_id] = OBJECT_ID(N'Shipment'))
BEGIN
    ALTER TABLE Shipment
	ADD ShippingTax MONEY NULL
END
GO

ALTER PROCEDURE [dbo].[ecf_Shipment_Insert]
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
	@SubTotal money
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
		[SubTotal]
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
		@SubTotal
	)

	SELECT @ShipmentId = SCOPE_IDENTITY()

	RETURN @@Error

GO

ALTER PROCEDURE [dbo].[ecf_Shipment_Update]
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
	@SubTotal money
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
		[SubTotal] = @SubTotal
	WHERE 
		[ShipmentId] = @ShipmentId

	RETURN @@Error
GO

-- create ShippingTax meta field
IF NOT EXISTS(SELECT * FROM [dbo].[MetaField] 
        WHERE [Name] = N'ShippingTax')
	AND EXISTS(SELECT TOP 1 MetaClassId from MetaClass WHERE Name = 'Shipment')
BEGIN
	DECLARE @metaClassId int, @metaDataTypeId int, @metaFieldId int
	SET @metaClassId = (SELECT TOP 1 MetaClassId from MetaClass WHERE Name = 'Shipment')
	SET @metaDataTypeId = (SELECT TOP 1 DataTypeId from MetaDataType WHERE Name = 'Money')
	
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
		   'ShippingTax'
           ,'Mediachase.Commerce.Orders.System.Shipment'
           ,@metaClassId
           ,'Shipping tax'
           ,'The shipping tax'
           ,@metaDataTypeId
           ,8
           ,1
           ,0
           ,0
           ,0
           ,0)
	
	SET @metaFieldId = (SELECT TOP 1 MetaFieldId from MetaField WHERE Name = 'ShippingTax')
	
	-- add relation between Shipment and ShippingTax
	INSERT INTO [dbo].[MetaClassMetaFieldRelation] ([MetaClassId], [MetaFieldId])
	VALUES (@metaClassId, @metaFieldId)
			   
	-- add relation between ShippingTax and Shipment's children
	 
	INSERT INTO [dbo].[MetaClassMetaFieldRelation] ([MetaClassId], [MetaFieldId])
	SELECT MC.MetaClassId, MF.MetaFieldId FROM MetaField MF, MetaClass MC
	WHERE MF.[SystemMetaClassId] = @metaClassId AND MF.MetaFieldId = @metaFieldId AND MC.ParentClassId = @metaClassId
 
END
GO

--beginUpdatingDatabaseVersion 
 
INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(6, 2, 0, GETUTCDATE()) 
GO 

--endUpdatingDatabaseVersion 
