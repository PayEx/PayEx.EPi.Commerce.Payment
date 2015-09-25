--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 6, @minor int = 0, @patch int = 0    
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
GO 

-- Create table AzureCompatible with default value is FALSE
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'AzureCompatible')
BEGIN
	CREATE TABLE dbo.AzureCompatible ( AzureCompatible bit)
	INSERT INTO dbo.AzureCompatible values (0)
END

-- Delete unused sql full-text search stored procedures
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_FullTextQueriesActivate]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_FullTextQueriesActivate]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_FullTextQueriesAddAllFields]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_FullTextQueriesAddAllFields]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_FullTextQueriesDeactivate]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_FullTextQueriesDeactivate]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_FullTextQueriesDeleteAllFields]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_FullTextQueriesDeleteAllFields]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_FullTextQueriesEnable]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_FullTextQueriesEnable]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_FullTextQueriesFieldUpdate]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_FullTextQueriesFieldUpdate]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_FullTextQueriesIndexUpdate]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_FullTextQueriesIndexUpdate]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_FullTextQueriesRepopulateAll]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_FullTextQueriesRepopulateAll]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_FullTextQueriesRepopulateCatalogScheduleCreate]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_FullTextQueriesRepopulateCatalogScheduleCreate]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_FullTextQueriesRepopulateCatalogScheduleDelete]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_FullTextQueriesRepopulateCatalogScheduleDelete]
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_FullTextQueriesUpdateAllFields]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_FullTextQueriesUpdateAllFields]
GO

-- Drop [dbo].[ecf_CreateFTSQuery]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_CreateFTSQuery]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_CreateFTSQuery]
GO

-- Drop [dbo].[ecf_ord_CreateFTSQuery]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_ord_CreateFTSQuery]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_ord_CreateFTSQuery]
GO

-- Update [dbo].[ecf_GetMostRecentOrder]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_GetMostRecentOrder]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_GetMostRecentOrder]
GO
CREATE PROCEDURE [dbo].[ecf_GetMostRecentOrder]
(
	@CustomerId uniqueidentifier, 
	@ApplicationId uniqueidentifier
)
AS
BEGIN
    declare @results udttOrderGroupId
    
    insert into @results (OrderGroupId)
	select top 1 [OrderGroupId]
	from [OrderGroup_PurchaseOrder] PO
	join OrderGroup OG on PO.ObjectId = OG.OrderGroupId
	where ([CustomerId] = @CustomerId) and ApplicationId = @ApplicationId
	ORDER BY ObjectId DESC

	exec dbo.ecf_Search_OrderGroup @results

	-- Return Purchase Order Details
	DECLARE @search_condition nvarchar(max)
	CREATE TABLE #OrderSearchResults(OrderGroupId int)
	insert into #OrderSearchResults select OrderGroupId from @results
	SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
	exec mdpsp_avto_OrderGroup_PurchaseOrder_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition

	DROP TABLE #OrderSearchResults
END
GO

-- Update [dbo].[ecf_Search_OrderGroup]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_OrderGroup]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_OrderGroup]
GO
CREATE PROCEDURE [dbo].[ecf_Search_OrderGroup]
    @results udttOrderGroupId readonly
AS
BEGIN

DECLARE @search_condition nvarchar(max)

-- Return GroupIds.
SELECT [OrderGroupId] FROM @results


-- Prevent any queries if order group doesn't exist
IF NOT EXISTS(SELECT * from OrderGroup G INNER JOIN @results R ON G.OrderGroupId = R.OrderGroupId)
	RETURN;

-- Return Order Form Collection
SELECT 'OrderForm' TableName, OE.*, O.*
	FROM [OrderFormEx] OE 
		INNER JOIN OrderForm O ON O.OrderFormId = OE.ObjectId 
		INNER JOIN @results R ON O.OrderGroupId = R.OrderGroupId 

if(@@ROWCOUNT = 0)
	RETURN;

-- Return Order Form Collection
SELECT 'OrderGroupAddress' TableName, OE.*, O.*
	FROM [OrderGroupAddressEx] OE 
		INNER JOIN OrderGroupAddress O ON O.OrderGroupAddressId = OE.ObjectId  
		INNER JOIN @results R ON O.OrderGroupId = R.OrderGroupId 

-- Return Shipment Collection
SELECT 'Shipment' TableName, SE.*, S.*
	FROM [ShipmentEx] SE 
		INNER JOIN Shipment S ON S.ShipmentId = SE.ObjectId 
		INNER JOIN @results R ON S.OrderGroupId = R.OrderGroupId 

-- Return Line Item Collection
SELECT 'LineItem' TableName, LE.*, L.*
	FROM [LineItemEx] LE 
		INNER JOIN LineItem L ON L.LineItemId = LE.ObjectId 
		INNER JOIN @results R ON L.OrderGroupId = R.OrderGroupId 

-- Return Order Form Payment Collection

CREATE TABLE #OrderSearchResults (OrderGroupId int)
insert into #OrderSearchResults (OrderGroupId) select OrderGroupId from @results
SET @search_condition = N'''INNER JOIN OrderFormPayment O ON O.PaymentId = T.ObjectId INNER JOIN #OrderSearchResults R ON O.OrderGroupId = R.OrderGroupId '''

DECLARE @metaclassid int
DECLARE @parentclassid int
DECLARE @parentmetaclassid int
DECLARE @rowNum int
DECLARE @maxrows int
DECLARE @tablename nvarchar(120)
DECLARE @name nvarchar(120)
DECLARE @procedurefull nvarchar(max)

SET @parentmetaclassid = (SELECT MetaClassId from [Metaclass] WHERE Name = N'orderformpayment' and TableName = N'orderformpayment')

SELECT top 1 @metaclassid = MetaClassId, @tablename = TableName, @parentclassid = ParentClassId, @name = Name from [Metaclass]
	SELECT @maxRows = count(*) from [Metaclass]
	SET @rowNum = 0
	WHILE @rowNum < @maxRows
	BEGIN
		SET @rowNum = @rowNum + 1
		IF (@parentclassid = @parentmetaclassid)
		BEGIN
			SET @procedurefull = N'mdpsp_avto_' + @tablename + N'_Search NULL, ' + N'''''''' + @tablename + N''''''+  ' TableName, [O].*'' ,'  + @search_condition
			EXEC (@procedurefull)
		END
		SELECT top 1 @metaclassid = MetaClassId, @tablename = TableName, @parentclassid = ParentClassId, @name = Name from [Metaclass] where MetaClassId > @metaclassid
	END

DROP TABLE #OrderSearchResults
-- Return Order Form Discount Collection
SELECT 'OrderFormDiscount' TableName, D.* 
	FROM [OrderFormDiscount] D 
		INNER JOIN @results R ON D.OrderGroupId = R.OrderGroupId 

-- Return Line Item Discount Collection
SELECT 'LineItemDiscount' TableName, D.* 
	FROM [LineItemDiscount] D 
		INNER JOIN @results R ON D.OrderGroupId = R.OrderGroupId 

-- Return Shipment Discount Collection
SELECT 'ShipmentDiscount' TableName, D.* 
	FROM [ShipmentDiscount] D 
		INNER JOIN @results R ON D.OrderGroupId = R.OrderGroupId 

-- assign random local variable to set @@rowcount attribute to 1
declare @temp as int
set @temp = 1

END
GO

-- Update [dbo].[ecf_Search_PaymentPlan]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_PaymentPlan]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_PaymentPlan]
GO
CREATE PROCEDURE [dbo].[ecf_Search_PaymentPlan]
    @ApplicationId				uniqueidentifier,
	@SQLClause 					nvarchar(max),
	@MetaSQLClause 				nvarchar(max),
	@FTSPhrase 					nvarchar(max),
    @AdvancedFTSPhrase 			nvarchar(max),
    @OrderBy 					nvarchar(max),
	@Namespace					nvarchar(1024) = N'',
	@Classes					nvarchar(max) = N'',
    @StartingRec 				int,
	@NumRecords   				int,
	@RecordCount                int OUTPUT
AS
BEGIN
    declare @results udttOrderGroupId
    insert into @results (OrderGroupId)    
    exec dbo.ecf_OrderSearch
        @ApplicationId, 
        @SQLClause, 
        @MetaSQLClause, 
        @FTSPhrase, 
        @AdvancedFTSPhrase, 
        @OrderBy, 
        @namespace, 
        @Classes, 
        @StartingRec, 
        @NumRecords, 
        @RecordCount output
	
	exec [dbo].[ecf_Search_OrderGroup] @results

    -- Return Purchase Order Details
	DECLARE @search_condition nvarchar(max)
	CREATE TABLE #OrderSearchResults (OrderGroupId int)
	insert into #OrderSearchResults select OrderGroupId from @results
	SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
	exec mdpsp_avto_OrderGroup_PaymentPlan_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition

	DROP TABLE #OrderSearchResults
END
GO

-- Update [dbo].[ecf_Search_PaymentPlan_Customer]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_PaymentPlan_Customer]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_PaymentPlan_Customer]
GO
CREATE PROCEDURE [dbo].[ecf_Search_PaymentPlan_Customer]
	@ApplicationId uniqueidentifier,
    @CustomerId uniqueidentifier
AS
BEGIN
	declare @results udttOrderGroupId
    
    insert into @results (OrderGroupId)    
    select OrderGroupId 
    from [OrderGroup_PaymentPlan] PO 
    join OrderGroup OG on PO.ObjectId = OG.OrderGroupId
    where ([CustomerId] = @CustomerId) and ApplicationId = @ApplicationId
        
    exec [dbo].[ecf_Search_OrderGroup] @results

    -- Return Purchase Order Details
	DECLARE @search_condition nvarchar(max)
	CREATE TABLE #OrderSearchResults (OrderGroupId int)
	insert into #OrderSearchResults select OrderGroupId from @results
	SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
	exec mdpsp_avto_OrderGroup_PaymentPlan_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition

	DROP TABLE #OrderSearchResults
END
GO

-- Update [dbo].[ecf_Search_PaymentPlan_CustomerAndName]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_PaymentPlan_CustomerAndName]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_PaymentPlan_CustomerAndName]
GO
CREATE PROCEDURE [dbo].[ecf_Search_PaymentPlan_CustomerAndName]
	@ApplicationId uniqueidentifier,
    @CustomerId uniqueidentifier,
	@Name nvarchar(64)
AS
BEGIN
	declare @results udttOrderGroupId
    
    insert into @results (OrderGroupId)    
    select OrderGroupId 
    from [OrderGroup_PaymentPlan] PO 
    join OrderGroup OG on PO.ObjectId = OG.OrderGroupId 
    where ([CustomerId] = @CustomerId) and [Name] = @Name and ApplicationId = @ApplicationId
    
    exec [dbo].[ecf_Search_OrderGroup] @results

    -- Return Purchase Order Details
	DECLARE @search_condition nvarchar(max)
	CREATE TABLE #OrderSearchResults (OrderGroupId int)
	insert into #OrderSearchResults select OrderGroupId from @results
	SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
	exec mdpsp_avto_OrderGroup_PaymentPlan_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition

	DROP TABLE #OrderSearchResults
END
GO

-- Update [dbo].[ecf_Search_PaymentPlan_CustomerAndOrderGroupId]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_PaymentPlan_CustomerAndOrderGroupId]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_PaymentPlan_CustomerAndOrderGroupId]
GO
CREATE PROCEDURE [dbo].[ecf_Search_PaymentPlan_CustomerAndOrderGroupId]
	@ApplicationId uniqueidentifier,
    @CustomerId uniqueidentifier,
    @OrderGroupId int
AS
BEGIN
	declare @results udttOrderGroupId
    
    insert into @results (OrderGroupId)    
    select OrderGroupId 
    from [OrderGroup_PaymentPlan] PO 
    join OrderGroup OG on PO.ObjectId = OG.OrderGroupId 
    where (PO.ObjectId = @OrderGroupId) and CustomerId = @CustomerId and ApplicationId = @ApplicationId
            
    exec [dbo].[ecf_Search_OrderGroup] @results

    -- Return Purchase Order Details
	DECLARE @search_condition nvarchar(max)
	CREATE TABLE #OrderSearchResults (OrderGroupId int)
	insert into #OrderSearchResults select OrderGroupId from @results
	SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
	exec mdpsp_avto_OrderGroup_PaymentPlan_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition

	DROP TABLE #OrderSearchResults
END
GO

-- Update [dbo].[ecf_Search_PurchaseOrder]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_PurchaseOrder]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_PurchaseOrder]
GO
CREATE PROCEDURE [dbo].[ecf_Search_PurchaseOrder]
    @ApplicationId				uniqueidentifier,
	@SQLClause 					nvarchar(max),
	@MetaSQLClause 				nvarchar(max),
	@FTSPhrase 					nvarchar(max),
    @AdvancedFTSPhrase 			nvarchar(max),
    @OrderBy 					nvarchar(max),
	@Namespace					nvarchar(1024) = N'',
	@Classes					nvarchar(max) = N'',
    @StartingRec 				int,
	@NumRecords   				int,
	@RecordCount                int OUTPUT
AS
BEGIN
    declare @results udttOrderGroupId
    insert into @results (OrderGroupId)    
    exec dbo.ecf_OrderSearch
        @ApplicationId, 
        @SQLClause, 
        @MetaSQLClause, 
        @FTSPhrase, 
        @AdvancedFTSPhrase, 
        @OrderBy, 
        @namespace, 
        @Classes, 
        @StartingRec, 
        @NumRecords, 
        @RecordCount output
	
	exec [dbo].[ecf_Search_OrderGroup] @results

    -- Return Purchase Order Details
	DECLARE @search_condition nvarchar(max)
	CREATE TABLE #OrderSearchResults (OrderGroupId int)
	insert into #OrderSearchResults select OrderGroupId from @results
	SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
	exec mdpsp_avto_OrderGroup_PurchaseOrder_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition

	DROP TABLE #OrderSearchResults
END
GO

-- Update [dbo].[ecf_Search_PurchaseOrder_Customer]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_PurchaseOrder_Customer]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_PurchaseOrder_Customer]
GO
CREATE PROCEDURE [dbo].[ecf_Search_PurchaseOrder_Customer]
	@ApplicationId uniqueidentifier,
    @CustomerId uniqueidentifier
AS
BEGIN
    declare @results udttOrderGroupId
    
    insert into @results (OrderGroupId)
	select [OrderGroupId]
	from [OrderGroup_PurchaseOrder] PO
	join OrderGroup OG on PO.ObjectId = OG.OrderGroupId
	where ([CustomerId] = @CustomerId) and ApplicationId = @ApplicationId
	
	exec dbo.ecf_Search_OrderGroup @results
	
	-- Return Purchase Order Details
	DECLARE @search_condition nvarchar(max)
	CREATE TABLE #OrderSearchResults (OrderGroupId int)
	insert into #OrderSearchResults (OrderGroupId) select OrderGroupId from @results
	SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
	exec mdpsp_avto_OrderGroup_PurchaseOrder_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition

	DROP TABLE #OrderSearchResults
END
GO

-- Update [dbo].[ecf_Search_PurchaseOrder_CustomerAndName]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_PurchaseOrder_CustomerAndName]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_PurchaseOrder_CustomerAndName]
GO
CREATE PROCEDURE [dbo].[ecf_Search_PurchaseOrder_CustomerAndName]
	@ApplicationId uniqueidentifier,
    @CustomerId uniqueidentifier,
	@Name nvarchar(64)
AS
BEGIN
    declare @results udttOrderGroupId
    
    insert into @results (OrderGroupId)
	select [OrderGroupId] 
	from [OrderGroup_PurchaseOrder] PO 
	join OrderGroup OG on PO.ObjectId = OG.OrderGroupId 
	where ([CustomerId] = @CustomerId) and [Name] = @Name and ApplicationId = @ApplicationId
	
	exec dbo.ecf_Search_OrderGroup @results
	
	-- Return Purchase Order Details
	DECLARE @search_condition nvarchar(max)
	CREATE TABLE #OrderSearchResults (OrderGroupId int)
	insert into #OrderSearchResults select OrderGroupId from @results
	SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
	exec mdpsp_avto_OrderGroup_PurchaseOrder_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition	

	DROP TABLE #OrderSearchResults
END
GO

-- Update [dbo].[ecf_Search_PurchaseOrder_CustomerAndOrderGroupId]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_PurchaseOrder_CustomerAndOrderGroupId]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_PurchaseOrder_CustomerAndOrderGroupId]
GO
CREATE PROCEDURE [dbo].[ecf_Search_PurchaseOrder_CustomerAndOrderGroupId]
	@ApplicationId uniqueidentifier,
    @CustomerId uniqueidentifier,
    @OrderGroupId int
AS
BEGIN
    declare @results udttOrderGroupId
    
    insert into @results (OrderGroupId)
	select [OrderGroupId]
	from [OrderGroup_PurchaseOrder] PO
	join OrderGroup OG on PO.ObjectId = OG.OrderGroupId 
	where (PO.ObjectId = @OrderGroupId) and CustomerId = @CustomerId and ApplicationId = @ApplicationId
	
	exec dbo.ecf_Search_OrderGroup @results
	
	-- Return Purchase Order Details
	DECLARE @search_condition nvarchar(max)
	CREATE TABLE #OrderSearchResults (OrderGroupId int)
	insert into #OrderSearchResults select OrderGroupId from @results
	SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
	exec mdpsp_avto_OrderGroup_PurchaseOrder_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition
		
	DROP TABLE #OrderSearchResults
END
GO

-- Update [dbo].[ecf_Search_ShoppingCart]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_ShoppingCart]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_ShoppingCart]
GO
CREATE PROCEDURE [dbo].[ecf_Search_ShoppingCart]
    @ApplicationId				uniqueidentifier,
	@SQLClause 					nvarchar(max),
	@MetaSQLClause 				nvarchar(max),
	@FTSPhrase 					nvarchar(max),
    @AdvancedFTSPhrase 			nvarchar(max),
    @OrderBy 					nvarchar(max),
	@Namespace					nvarchar(1024) = N'',
	@Classes					nvarchar(max) = N'',
    @StartingRec 				int,
	@NumRecords   				int,
	@RecordCount                int OUTPUT
AS
BEGIN
    declare @results udttOrderGroupId
    insert into @results (OrderGroupId)    
    exec dbo.ecf_OrderSearch
        @ApplicationId, 
        @SQLClause, 
        @MetaSQLClause, 
        @FTSPhrase, 
        @AdvancedFTSPhrase, 
        @OrderBy, 
        @namespace, 
        @Classes, 
        @StartingRec, 
        @NumRecords, 
        @RecordCount output
    
    exec dbo.ecf_Search_OrderGroup @results
    
	IF(EXISTS(SELECT OrderGroupId from OrderGroup where OrderGroupId IN (SELECT [OrderGroupId] FROM @results)))
	begin
	    -- Return Purchase Order Details
		DECLARE @search_condition nvarchar(max)
		CREATE TABLE #OrderSearchResults (OrderGroupId int)
		insert into #OrderSearchResults (OrderGroupId) select OrderGroupId from @results
		SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
		exec mdpsp_avto_OrderGroup_ShoppingCart_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition

		DROP TABLE #OrderSearchResults
	end
END
GO

-- Update [dbo].[ecf_Search_ShoppingCart_Customer]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_ShoppingCart_Customer]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_ShoppingCart_Customer]
GO
CREATE PROCEDURE [dbo].[ecf_Search_ShoppingCart_Customer]
	@ApplicationId uniqueidentifier,
	@CustomerId uniqueidentifier
AS
BEGIN
    declare @results udttOrderGroupId
    
    insert into @results (OrderGroupId)
	select [OrderGroupId]
	from [OrderGroup_ShoppingCart] PO 
	join OrderGroup OG on PO.ObjectId = OG.OrderGroupId 
	where ([CustomerId] = @CustomerId) and ApplicationId = @ApplicationId
	
	exec dbo.ecf_Search_OrderGroup @results
	
	IF(EXISTS(SELECT OrderGroupId from OrderGroup where OrderGroupId IN (SELECT [OrderGroupId] FROM @results)))
	begin
	    -- Return Purchase Order Details
		DECLARE @search_condition nvarchar(max)
		CREATE TABLE #OrderSearchResults (OrderGroupId int)
		insert into #OrderSearchResults (OrderGroupId) select OrderGroupId from @results
		SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
		exec mdpsp_avto_OrderGroup_ShoppingCart_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition
        
		DROP TABLE #OrderSearchResults
	end
END
GO

-- Update [dbo].[ecf_Search_ShoppingCart_CustomerAndName]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_ShoppingCart_CustomerAndName]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_ShoppingCart_CustomerAndName]
GO
CREATE PROCEDURE [dbo].[ecf_Search_ShoppingCart_CustomerAndName]
	@ApplicationId uniqueidentifier,
    @CustomerId uniqueidentifier,
	@Name nvarchar(64) = null
AS
BEGIN
    declare @results udttOrderGroupId
    
    insert into @results (OrderGroupId)
	select [OrderGroupId]
	from [OrderGroup_ShoppingCart] PO
	join OrderGroup OG on PO.ObjectId = OG.OrderGroupId
	where ([CustomerId] = @CustomerId) and [Name] = @Name and ApplicationId = @ApplicationId

    exec dbo.ecf_Search_OrderGroup @results

	IF(EXISTS(SELECT OrderGroupId from OrderGroup where OrderGroupId IN (SELECT [OrderGroupId] FROM @results)))
	begin
	    -- Return Purchase Order Details
		DECLARE @search_condition nvarchar(max)
		CREATE TABLE #OrderSearchResults (OrderGroupId int)
		insert into #OrderSearchResults (OrderGroupId) select OrderGroupId from @results
		SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
		exec mdpsp_avto_OrderGroup_ShoppingCart_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition

		DROP TABLE #OrderSearchResults
	end
END
GO

-- Update [dbo].[ecf_Search_ShoppingCart_CustomerAndOrderGroupId]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_ShoppingCart_CustomerAndOrderGroupId]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_ShoppingCart_CustomerAndOrderGroupId]
GO
CREATE PROCEDURE [dbo].[ecf_Search_ShoppingCart_CustomerAndOrderGroupId]
	@ApplicationId uniqueidentifier,
    @CustomerId uniqueidentifier,
    @OrderGroupId int
AS
BEGIN
	declare @results udttOrderGroupId
    
    insert into @results (OrderGroupId)
	select [OrderGroupId]
	from [OrderGroup_ShoppingCart] C
	join OrderGroup OG on C.ObjectId = OG.OrderGroupId
	where (C.ObjectId = @OrderGroupId) and CustomerId = @CustomerId and ApplicationId = @ApplicationId
	
	exec dbo.ecf_Search_OrderGroup @results
	
	IF(EXISTS(SELECT OrderGroupId from OrderGroup where OrderGroupId IN (SELECT [OrderGroupId] FROM @results)))
	begin
	    -- Return Purchase Order Details
		DECLARE @search_condition nvarchar(max)
		CREATE TABLE #OrderSearchResults (OrderGroupId int)
		insert into #OrderSearchResults (OrderGroupId) select OrderGroupId from @results
		SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
		exec mdpsp_avto_OrderGroup_ShoppingCart_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition
        
		DROP TABLE #OrderSearchResults
	end
END
GO

-- Update [dbo].[mdpsp_sys_AddMetaFieldToMetaClass]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_AddMetaFieldToMetaClass]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_AddMetaFieldToMetaClass]
GO
CREATE PROCEDURE [dbo].[mdpsp_sys_AddMetaFieldToMetaClass]
	@MetaClassId	INT,
	@MetaFieldId	INT,
	@Weight	INT
AS
BEGIN
	-- Step 0. Prepare
	SET NOCOUNT ON

	DECLARE @IsAbstractClass	BIT
	SELECT @IsAbstractClass = IsAbstract FROM MetaClass WHERE MetaClassId = @MetaClassId

    BEGIN TRAN
	IF NOT EXISTS( SELECT * FROM MetaClass WHERE MetaClassId = @MetaClassId AND IsSystem = 0)
	BEGIN
		RAISERROR ('Wrong @MetaClassId. The class is system or not exists.', 16,1)
		GOTO ERR
	END

	IF NOT EXISTS( SELECT * FROM MetaField WHERE MetaFieldId = @MetaFieldId AND SystemMetaClassId = 0)
	BEGIN
		RAISERROR ('Wrong @MetaFieldId. The field is system or not exists.', 16,1)
		GOTO ERR
	END

	IF @IsAbstractClass = 0
	BEGIN
		-- Step 1. Insert a new column.
		DECLARE @Name		NVARCHAR(256)
		DECLARE @DataTypeId	INT
		DECLARE @Length		INT
		DECLARE @AllowNulls		BIT
		DECLARE @MultiLanguageValue BIT
		DECLARE @AllowSearch	BIT
		DECLARE @IsEncrypted	BIT

		SELECT @Name = [Name], @DataTypeId = DataTypeId,  @Length = [Length], @AllowNulls = AllowNulls, @MultiLanguageValue = MultiLanguageValue, @AllowSearch = AllowSearch, @IsEncrypted = IsEncrypted
		FROM [MetaField]
        WHERE MetaFieldId = @MetaFieldId AND SystemMetaClassId = 0

		-- Step 1-1. Create a new column query.

		DECLARE @MetaClassTableName NVARCHAR(256)
		DECLARE @SqlDataTypeName NVARCHAR(256)
		DECLARE @IsVariableDataType BIT
		DECLARE @DefaultValue	NVARCHAR(50)

		SELECT @MetaClassTableName = TableName FROM MetaClass WHERE MetaClassId = @MetaClassId

		IF @@ERROR<> 0 GOTO ERR

		SELECT @SqlDataTypeName = SqlName,  @IsVariableDataType = Variable, @DefaultValue = DefaultValue FROM MetaDataType WHERE DataTypeId= @DataTypeId

		IF @@ERROR<> 0 GOTO ERR

		DECLARE @ExecLine 			NVARCHAR(1024)
		DECLARE @ExecLineLocalization 	NVARCHAR(1024)

		SET @ExecLine = 'ALTER TABLE [dbo].['+@MetaClassTableName+'] ADD ['+@Name+'] ' + @SqlDataTypeName
		SET @ExecLineLocalization = 'ALTER TABLE [dbo].['+@MetaClassTableName+'_Localization] ADD ['+@Name+'] ' + @SqlDataTypeName

		IF @IsVariableDataType = 1
		BEGIN
			SET @ExecLine = @ExecLine + ' (' + STR(@Length) + ')'
			SET @ExecLineLocalization = @ExecLineLocalization + ' (' + STR(@Length) + ')'
		END
		ELSE
		BEGIN
			IF @DataTypeId = 5 OR @DataTypeId = 24
			BEGIN
				DECLARE @MdpPrecision NVARCHAR(10)
				DECLARE @MdpScale NVARCHAR(10)

				SET @MdpPrecision = NULL
				SET @MdpScale = NULL

				SELECT @MdpPrecision = [Value] FROM MetaAttribute
				WHERE
					AttrOwnerId = @MetaFieldId AND
					AttrOwnerType = 2 AND
					[Key] = 'MdpPrecision'

				SELECT @MdpScale = [Value] FROM MetaAttribute
				WHERE
					AttrOwnerId = @MetaFieldId AND
					AttrOwnerType = 2 AND
					[Key] = 'MdpScale'

				IF @MdpPrecision IS NOT NULL AND @MdpScale IS NOT NULL
				BEGIN
					SET @ExecLine = @ExecLine + ' (' + @MdpPrecision + ',' + @MdpScale + ')'
					SET @ExecLineLocalization = @ExecLineLocalization + ' (' + @MdpPrecision + ',' + @MdpScale + ')'
				END
			END
		END

		SET @ExecLineLocalization = @ExecLineLocalization + ' NULL'

		IF @AllowNulls = 1
		BEGIN
			SET @ExecLine = @ExecLine + ' NULL'
		END
		ELSE
			BEGIN
				SET @ExecLine = @ExecLine + ' NOT NULL DEFAULT ' + @DefaultValue

				SET @ExecLine = @ExecLine  +'  WITH VALUES'
			END

		-- Step 1-2. Create a new column.
		EXEC (@ExecLine)

		IF @@ERROR<> 0 GOTO ERR

		-- Step 1-3. Create a new localization column.
		EXEC (@ExecLineLocalization)

		IF @@ERROR <> 0 GOTO ERR
	END

	-- Step 2. Insert a record in to MetaClassMetaFieldRelation table.
	INSERT INTO [MetaClassMetaFieldRelation] (MetaClassId, MetaFieldId, Weight) VALUES(@MetaClassId, @MetaFieldId, @Weight)

	IF @@ERROR <> 0 GOTO ERR

	IF @IsAbstractClass = 0
	BEGIN
		EXEC mdpsp_sys_CreateMetaClassProcedure @MetaClassId

		IF @@ERROR <> 0 GOTO ERR
	END

	COMMIT TRAN

    RETURN

ERR:
	ROLLBACK TRAN
    RETURN
END
GO

-- Update [dbo].[mdpsp_sys_DeleteMetaFieldFromMetaClass]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_DeleteMetaFieldFromMetaClass]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_DeleteMetaFieldFromMetaClass]
GO
CREATE PROCEDURE [dbo].[mdpsp_sys_DeleteMetaFieldFromMetaClass]
	@MetaClassId	INT,
	@MetaFieldId	INT
AS
BEGIN
	IF NOT EXISTS(SELECT * FROM MetaClassMetaFieldRelation WHERE MetaFieldId = @MetaFieldId AND MetaClassId = @MetaClassId)
	BEGIN
		RETURN
	END

	-- Step 0. Prepare
	SET NOCOUNT ON

	DECLARE @MetaFieldName NVARCHAR(256)
	DECLARE @MetaFieldOwnerTable NVARCHAR(256)
	DECLARE @BaseMetaFieldOwnerTable NVARCHAR(256)
	DECLARE @IsAbstractClass BIT

	-- Step 1. Find a Field Name
	-- Step 2. Find a TableName
	IF NOT EXISTS(SELECT * FROM MetaField MF WHERE MetaFieldId = @MetaFieldId AND SystemMetaClassId = 0 )
	BEGIN
		RAISERROR ('Wrong @MetaFieldId.', 16, 1)
		GOTO ERR
	END

	SELECT @MetaFieldName = MF.[Name] FROM MetaField MF WHERE MetaFieldId = @MetaFieldId AND SystemMetaClassId = 0

	IF NOT EXISTS(SELECT * FROM MetaClass MC WHERE MetaClassId = @MetaClassId AND IsSystem = 0)
	BEGIN
		RAISERROR ('Wrong @MetaClassId.', 16, 1)
		GOTO ERR
	END

	SELECT @BaseMetaFieldOwnerTable = MC.TableName, @IsAbstractClass = MC.IsAbstract FROM MetaClass MC
		WHERE MetaClassId = @MetaClassId AND IsSystem = 0

	SET @MetaFieldOwnerTable = @BaseMetaFieldOwnerTable

	 IF @@ERROR <> 0 GOTO ERR

	BEGIN TRAN

	IF @IsAbstractClass = 0
	BEGIN
		EXEC mdpsp_sys_DeleteMetaKeyObjects @MetaClassId, @MetaFieldId
		 IF @@ERROR <> 0 GOTO ERR

		-- Step 3. Delete Constrains
		EXEC mdpsp_sys_DeleteDContrainByTableAndField @MetaFieldOwnerTable, @MetaFieldName

		IF @@ERROR <> 0 GOTO ERR

		-- Step 4. Delete Field
		EXEC ('ALTER TABLE ['+@MetaFieldOwnerTable+'] DROP COLUMN [' + @MetaFieldName + ']')

		IF @@ERROR <> 0 GOTO ERR

		-- Update 2007/10/05: Remove meta field from Localization table (if table exists)
		SET @MetaFieldOwnerTable = @BaseMetaFieldOwnerTable + '_Localization'

		if exists (select * from dbo.sysobjects where id = object_id(@MetaFieldOwnerTable) and OBJECTPROPERTY(id, N'IsUserTable') = 1)
		begin
			-- a). Delete constraints
			EXEC mdpsp_sys_DeleteDContrainByTableAndField @MetaFieldOwnerTable, @MetaFieldName
			-- a). Drop column
			EXEC ('ALTER TABLE ['+@MetaFieldOwnerTable+'] DROP COLUMN [' + @MetaFieldName + ']')
		end
	END

	-- Step 5. Delete Field Info Record
	DELETE FROM MetaClassMetaFieldRelation WHERE MetaFieldId = @MetaFieldId AND MetaClassId = @MetaClassId
	IF @@ERROR <> 0 GOTO ERR

	IF @IsAbstractClass = 0
	BEGIN
		EXEC mdpsp_sys_CreateMetaClassProcedure @MetaClassId

		IF @@ERROR <> 0 GOTO ERR
	END

	COMMIT TRAN
	RETURN
ERR:
	ROLLBACK TRAN

	RETURN @@Error
END
GO

-- Update [dbo].[mdpsp_sys_MetaFieldAllowSearch]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_MetaFieldAllowSearch]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_MetaFieldAllowSearch]
GO
CREATE PROCEDURE dbo.mdpsp_sys_MetaFieldAllowSearch
    @MetaFieldId int,
    @AllowSearch bit
as
begin
    set nocount on

    if not exists (select 1 from MetaField where MetaFieldId = @MetaFieldId)
    begin
        raiserror('The specified meta field does not exists or is a system field.', 16,1)
    end
    else
    begin
        update MetaField
        set AllowSearch = @AllowSearch
        where MetaFieldId = @MetaFieldId
    end
end
GO

-- Update [dbo].[ecf_CatalogEntrySearch]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_CatalogEntrySearch]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_CatalogEntrySearch]
GO
CREATE PROCEDURE [dbo].[ecf_CatalogEntrySearch]
(
	@ApplicationId				uniqueidentifier,
	@SearchSetId				uniqueidentifier,
	@Language 					nvarchar(50),
	@Catalogs 					nvarchar(max),
	@CatalogNodes 				nvarchar(max),
	@SQLClause 					nvarchar(max),
	@MetaSQLClause 				nvarchar(max),
	@KeywordPhrase				nvarchar(max),
	@OrderBy 					nvarchar(max),
	@Namespace					nvarchar(1024) = N'',
	@Classes					nvarchar(max) = N'',
	@StartingRec				int,
	@NumRecords					int,
	@JoinType					nvarchar(50),
	@SourceTableName			sysname,
	@TargetQuery				nvarchar(max),
	@SourceJoinKey				sysname,
	@TargetJoinKey				sysname,
	@RecordCount				int OUTPUT,
	@ReturnTotalCount			bit = 1
)
AS

BEGIN
	SET NOCOUNT ON
	
	DECLARE @FilterVariables_tmp 		nvarchar(max)
	DECLARE @query_tmp 		nvarchar(max)
	DECLARE @FilterQuery_tmp 		nvarchar(max)
	DECLARE @TableName_tmp sysname
	declare @SelectMetaQuery_tmp nvarchar(max)
	declare @FromQuery_tmp nvarchar(max)
	declare @SelectCountQuery_tmp nvarchar(max)
	declare @FullQuery nvarchar(max)
	DECLARE @JoinQuery_tmp 		nvarchar(max)
	DECLARE @TempTableName_tmp 		sysname
	DECLARE @NameSearchQuery nvarchar(max)

	-- Precalculate length for constant strings
	DECLARE @MetaSQLClauseLength bigint
	DECLARE @KeywordPhraseLength bigint
	SET @MetaSQLClauseLength = LEN(@MetaSQLClause)
	SET @KeywordPhraseLength = LEN(@KeywordPhrase)

	set @RecordCount = -1

	-- ######## CREATE FILTER QUERY
	-- CREATE "JOINS" NEEDED
	-- Create filter query
	set @FilterQuery_tmp = N''

	-- Only add NodeEntryRelation table join if one Node filter is specified, if more than one then we can't inner join it
	if(Len(@CatalogNodes) != 0 and (select count(Item) from ecf_splitlist(@CatalogNodes)) <= 1)
	begin
		set @FilterQuery_tmp = @FilterQuery_tmp + N' INNER JOIN NodeEntryRelation NodeEntryRelation ON CatalogEntry.CatalogEntryId = NodeEntryRelation.CatalogEntryId'
	end

	-- CREATE "WHERE" NEEDED
	set @FilterQuery_tmp = @FilterQuery_tmp + N' WHERE CatalogEntry.ApplicationId = ''' + cast(@ApplicationId as nvarchar(100)) + ''' AND '
	
	-- If nodes specified, no need to filter by catalog since that is done in node filter
	if(Len(@CatalogNodes) = 0)
	begin
		set @FilterQuery_tmp = @FilterQuery_tmp + N' CatalogEntry.CatalogId in (select * from @Catalogs_temp)'
	end

	-- Different filter if more than one category is specified
	if(Len(@CatalogNodes) != 0 and (select count(Item) from ecf_splitlist(@CatalogNodes)) > 1)
	begin
		set @FilterQuery_tmp = @FilterQuery_tmp + N' CatalogEntry.CatalogEntryId in (select NodeEntryRelation.CatalogEntryId from NodeEntryRelation NodeEntryRelation where '
	end

	-- Add node filter, have to do this way to not produce multiple entry items
	if(Len(@CatalogNodes) != 0)
	begin
		set @FilterQuery_tmp = @FilterQuery_tmp + N' NodeEntryRelation.CatalogNodeId IN (select CatalogNode.CatalogNodeId from CatalogNode CatalogNode'
		set @FilterQuery_tmp = @FilterQuery_tmp + N' WHERE (CatalogNode.[Code] in (select Item from ecf_splitlist(''' + @CatalogNodes + '''))) AND NodeEntryRelation.CatalogId in (select * from @Catalogs_temp)'
		set @FilterQuery_tmp = @FilterQuery_tmp + N')'
	end

	-- Different filter if more than one category is specified
	if(Len(@CatalogNodes) != 0 and (select count(Item) from ecf_splitlist(@CatalogNodes)) > 1)
	begin
		set @FilterQuery_tmp = @FilterQuery_tmp + N')'
	end

	-- add sql clause statement here, if specified
	if(Len(@SQLClause) != 0)
	begin
		set @FilterQuery_tmp = @FilterQuery_tmp + N' AND (' + @SqlClause + ')'
	end

	-- 1. Cycle through all the available product meta classes
	DECLARE MetaClassCursor CURSOR READ_ONLY
	FOR SELECT C.TableName FROM MetaClass C INNER JOIN MetaClass C2 ON C.ParentClassId = C2.MetaClassId
		WHERE C.Namespace like @Namespace + '%' AND (C.[Name] in (select Item from ecf_splitlist(@Classes)) or @Classes = '')
		and C.IsSystem = 0 and C2.[Name] = 'CatalogEntry'

	OPEN MetaClassCursor
	FETCH NEXT FROM MetaClassCursor INTO @TableName_tmp
	WHILE (@@fetch_status = 0)
	BEGIN 
		IF(@KeywordPhraseLength>0)
			-- Search by Name in CatalogEntry
			SET @Query_tmp = 'SELECT META.ObjectId AS ''Key'', 100 AS ''Rank'' FROM ' + @TableName_tmp + ' META JOIN CatalogEntry ON CatalogEntry.CatalogEntryId = META.ObjectId WHERE CatalogEntry.Name LIKE N''%' + @KeywordPhrase + '%'''
		ELSE
			set @Query_tmp = 'select META.ObjectId as ''Key'', 100 as ''Rank'' from ' + @TableName_tmp + ' META' -- INNER JOIN ' + @TableName_tmp + '_Localization LOC ON META.ObjectId = LOC.Id'
		
		-- Add meta Where clause
		if(@MetaSQLClauseLength>0)
			set @query_tmp = @query_tmp + ' WHERE ' + @MetaSQLClause

		if(@SelectMetaQuery_tmp is null)
			set @SelectMetaQuery_tmp = @Query_tmp;
		else
			set @SelectMetaQuery_tmp = @SelectMetaQuery_tmp + N' UNION ALL ' + @Query_tmp;

	FETCH NEXT FROM MetaClassCursor INTO @TableName_tmp
	END
	CLOSE MetaClassCursor
	DEALLOCATE MetaClassCursor

	IF(@KeywordPhraseLength>0)
		SET @NameSearchQuery = N' UNION SELECT CatalogEntry.CatalogEntryId AS ''Key'', 100 AS ''Rank'' FROM CatalogEntry WHERE CatalogEntry.Name LIKE N''%' + @KeywordPhrase + '%'' ';	
	ELSE
		SET @NameSearchQuery = N'';

	-- Create from command
	SET @FromQuery_tmp = N'FROM [CatalogEntry] CatalogEntry' + N' INNER JOIN (select distinct U.[KEY], MIN(U.Rank) AS Rank from (' + @SelectMetaQuery_tmp + @NameSearchQuery + N') U GROUP BY U.[KEY]) META ON CatalogEntry.[CatalogEntryId] = META.[KEY] '

	-- attach inner join if needed
	if(@JoinType is not null and Len(@JoinType) > 0)
	begin
		set @Query_tmp = ''
		EXEC [ecf_CreateTableJoinQuery] @SourceTableName, @TargetQuery, @SourceJoinKey, @TargetJoinKey, @JoinType, @Query_tmp OUT
		print(@Query_tmp)
		set @FromQuery_tmp = @FromQuery_tmp + N' ' + @Query_tmp
	end
	
	-- order by statement here
	if(Len(@OrderBy) = 0 and Len(@CatalogNodes) != 0 and CHARINDEX(',', @CatalogNodes) = 0)
	begin
		set @OrderBy = 'NodeEntryRelation.SortOrder'
	end
	else if(Len(@OrderBy) = 0)
	begin
		set @OrderBy = 'CatalogEntry.CatalogEntryId'
	end

	-- add catalogs temp variable that will be used to filter out catalogs
	set @FilterVariables_tmp = 'declare @Catalogs_temp table (CatalogId int);'
	set @FilterVariables_tmp = @FilterVariables_tmp + 'INSERT INTO @Catalogs_temp select CatalogId from Catalog'
	if(Len(RTrim(LTrim(@Catalogs)))>0)
		set @FilterVariables_tmp = @FilterVariables_tmp + ' WHERE ([Catalog].[Name] in (select Item from ecf_splitlist(''' + @Catalogs + ''')))'
	set @FilterVariables_tmp = @FilterVariables_tmp + ';'

	if(@ReturnTotalCount = 1) -- Only return count if we requested it
		begin
			set @FullQuery = N'SELECT count([CatalogEntry].CatalogEntryId) OVER() TotalRecords, [CatalogEntry].CatalogEntryId, Rank, ROW_NUMBER() OVER(ORDER BY ' + @OrderBy + N') RowNumber ' + @FromQuery_tmp + @FilterQuery_tmp
			-- use temp table variable
			set @FullQuery = N'with OrderedResults as (' + @FullQuery +') INSERT INTO @Page_temp (TotalRecords, ObjectId, SortOrder) SELECT top(' + cast(@NumRecords as nvarchar(50)) + ') TotalRecords, CatalogEntryId, RowNumber FROM OrderedResults WHERE RowNumber > ' + cast(@StartingRec as nvarchar(50)) + ';'
			set @FullQuery = @FilterVariables_tmp + 'declare @Page_temp table (TotalRecords int,ObjectId int,SortOrder int);' + @FullQuery + ';select @RecordCount = TotalRecords from @Page_temp;INSERT INTO CatalogEntrySearchResults (SearchSetId, CatalogEntryId, SortOrder) SELECT ''' + cast(@SearchSetId as nvarchar(100)) + N''', ObjectId, SortOrder from @Page_temp;'
			exec sp_executesql @FullQuery, N'@RecordCount int output', @RecordCount = @RecordCount OUTPUT
		end
	else
		begin
			-- simplified query with no TotalRecords, should give some performance gain
			set @FullQuery = N'SELECT [CatalogEntry].CatalogEntryId, Rank, ROW_NUMBER() OVER(ORDER BY ' + @OrderBy + N') RowNumber ' + @FromQuery_tmp + @FilterQuery_tmp
			
			set @FullQuery = @FilterVariables_tmp + N'with OrderedResults as (' + @FullQuery +') INSERT INTO CatalogEntrySearchResults (SearchSetId, CatalogEntryId, SortOrder) SELECT top(' + cast(@NumRecords as nvarchar(50)) + ') ''' + cast(@SearchSetId as nvarchar(100)) + N''', CatalogEntryId, RowNumber FROM OrderedResults WHERE RowNumber > ' + cast(@StartingRec as nvarchar(50)) + ';'
			exec(@FullQuery)
		end
	SET NOCOUNT OFF
END
GO

-- Update [dbo].[ecf_CatalogNodesList]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_CatalogNodesList]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_CatalogNodesList]
GO
CREATE PROCEDURE [dbo].[ecf_CatalogNodesList]
(
	@CatalogId int,
	@CatalogNodeId int,
	@EntryMetaSQLClause nvarchar(max),
	@OrderClause nvarchar(100),
	@StartingRec int,
	@NumRecords int,
	@ReturnInactive bit = 0,
	@ReturnTotalCount bit = 1
)
AS

BEGIN
	SET NOCOUNT ON

	declare @execStmtString nvarchar(max)
	declare @selectStmtString nvarchar(max)
	declare @query_tmp nvarchar(max)
	declare @EntryMetaSQLClauseLength bigint
	declare @TableName_tmp sysname
	declare @SelectEntryMetaQuery_tmp nvarchar(max)
	set @EntryMetaSQLClauseLength = LEN(@EntryMetaSQLClause)

	set @execStmtString=N''

	-- assign ORDER BY statement if it is empty
	if(Len(RTrim(LTrim(@OrderClause))) = 0)
		set @OrderClause = N'ID ASC'

    -- Construct meta class joins for CatalogEntry table if a WHERE clause has been specified for Entry Meta data
    IF(@EntryMetaSQLClauseLength>0)
    BEGIN
    	-- If there is a meta SQL clause provided, cycle through all the available product meta classes
    	--print 'Iterating through entry meta classes'
    	-- Similar to [ecf_CatalogEntrySearch], but simpler due to fewer variations, i.e.:
    	--   No @Classes parameter
    	--   No @Namespace
    	-- Left in the commented out localization join for future reference
    	DECLARE MetaClassCursor CURSOR READ_ONLY
    	FOR SELECT C.TableName FROM MetaClass C INNER JOIN MetaClass C2 ON C.ParentClassId = C2.MetaClassId
    		WHERE C.IsSystem = 0 and C2.[Name] = 'CatalogEntry'
    
    	OPEN MetaClassCursor
    	FETCH NEXT FROM MetaClassCursor INTO @TableName_tmp
    	WHILE (@@fetch_status = 0)
    	BEGIN 
    		--print 'Metaclass Table: ' + @TableName_tmp
            set @Query_tmp = 'select META.ObjectId as ''Key'', 100 as ''Rank'' from ' + @TableName_tmp + ' META'
    		set @query_tmp = @query_tmp + ' WHERE ' + @EntryMetaSQLClause
    		
    		-- Add meta Where clause
    
    		if(@SelectEntryMetaQuery_tmp is null)
    			set @SelectEntryMetaQuery_tmp = @Query_tmp;
    		else
    			set @SelectEntryMetaQuery_tmp = @SelectEntryMetaQuery_tmp + N' UNION ALL ' + @Query_tmp;
    
    	FETCH NEXT FROM MetaClassCursor INTO @TableName_tmp
    	END
    	CLOSE MetaClassCursor
	    DEALLOCATE MetaClassCursor

		set @SelectEntryMetaQuery_tmp = N' INNER JOIN (select distinct U.[KEY], MIN(U.Rank) AS Rank from (' + @SelectEntryMetaQuery_tmp + N') U GROUP BY U.[KEY]) META ON CE.[CatalogEntryId] = META.[KEY] '
    END
    ELSE
    BEGIN
        set @SelectEntryMetaQuery_tmp = N''
    END

	if (COALESCE(@CatalogNodeId, 0)=0)
	begin
		set @selectStmtString=N'select SEL.*, row_number() over(order by '+ @OrderClause +N') as RowNumber
				from
				(
					-- select Catalog Nodes
					SELECT CN.[CatalogNodeId] as ID, CN.[Name], ''Node'' as Type, CN.[Code], CN.[StartDate], CN.[EndDate], CN.[IsActive], CN.[SortOrder], OG.[NAME] as Owner
						FROM [CatalogNode] CN 
							JOIN Catalog C ON (CN.CatalogId = C.CatalogId)
                            LEFT JOIN cls_Organization OG ON (OG.OrganizationId = C.Owner)
						WHERE CatalogNodeId IN
						(SELECT DISTINCT N.CatalogNodeId from [CatalogNode] N
							LEFT OUTER JOIN CatalogNodeRelation NR ON N.CatalogNodeId = NR.ChildNodeId
							WHERE
							(
								(N.CatalogId = @CatalogId AND N.ParentNodeId = @CatalogNodeId)
								OR
								(NR.CatalogId = @CatalogId AND NR.ParentNodeId = @CatalogNodeId)
							)
							AND
							((N.IsActive = 1) or @ReturnInactive = 1)
						)

					UNION

					-- select Catalog Entries
					SELECT CE.[CatalogEntryId] as ID, CE.[Name], CE.ClassTypeId as Type, CE.[Code], CE.[StartDate], CE.[EndDate], CE.[IsActive], 0, OG.[NAME] as Owner
						FROM [CatalogEntry] CE
							JOIN Catalog C ON (CE.CatalogId = C.CatalogId)'
							+ @SelectEntryMetaQuery_tmp
							+ N'
                            LEFT JOIN cls_Organization OG ON (OG.OrganizationId = C.Owner)
					WHERE
						CE.CatalogId = @CatalogId AND
						NOT EXISTS(SELECT * FROM NodeEntryRelation R WHERE R.CatalogId = @CatalogId and CE.CatalogEntryId = R.CatalogEntryId) AND
						((CE.IsActive = 1) or @ReturnInactive = 1)
				) SEL'
	end
	else
	begin

		-- Get the original catalog id for the given catalog node
		SELECT @CatalogId = [CatalogId] FROM [CatalogNode] WHERE [CatalogNodeId] = @CatalogNodeId

		set @selectStmtString=N'select SEL.*, row_number() over(order by '+ @OrderClause +N') as RowNumber
			from
			(
				-- select Catalog Nodes
				SELECT CN.[CatalogNodeId] as ID, CN.[Name], ''Node'' as Type, CN.[Code], CN.[StartDate], CN.[EndDate], CN.[IsActive], CN.[SortOrder], OG.[NAME] as Owner
					FROM [CatalogNode] CN 
						JOIN Catalog C ON (CN.CatalogId = C.CatalogId)
						--We actually dont need to join NodeEntryRelation to get the SortOrder because it is always 0
                        --JOIN CatalogEntry CE ON CE.CatalogId = C.CatalogId
						--LEFT JOIN NodeEntryRelation NER ON (NER.CatalogId = CN.CatalogId And NER.CatalogNodeId = CN.CatalogNodeId  AND CE.CatalogEntryId = NER.CatalogEntryId ) 
                        LEFT JOIN cls_Organization OG ON (OG.OrganizationId = C.Owner)
					WHERE CN.CatalogNodeId IN
				(SELECT DISTINCT N.CatalogNodeId from [CatalogNode] N
				LEFT OUTER JOIN CatalogNodeRelation NR ON N.CatalogNodeId = NR.ChildNodeId
				WHERE
					((N.CatalogId = @CatalogId AND N.ParentNodeId = @CatalogNodeId) OR (NR.CatalogId = @CatalogId AND NR.ParentNodeId = @CatalogNodeId)) AND
					((N.IsActive = 1) or @ReturnInactive = 1))

				UNION
				
				-- select Catalog Entries
				SELECT CE.[CatalogEntryId] as ID, CE.[Name], CE.ClassTypeId as Type, CE.[Code], CE.[StartDate], CE.[EndDate], CE.[IsActive], R.[SortOrder], OG.[NAME] as Owner
					FROM [CatalogEntry] CE
						JOIN Catalog C ON (CE.CatalogId = C.CatalogId)
						JOIN NodeEntryRelation R ON R.CatalogEntryId = CE.CatalogEntryId'
							+ @SelectEntryMetaQuery_tmp
							+ N'
                        LEFT JOIN cls_Organization OG ON (OG.OrganizationId = C.Owner)
				WHERE
					R.CatalogNodeId = @CatalogNodeId AND
					R.CatalogId = @CatalogId AND
						((CE.IsActive = 1) or @ReturnInactive = 1)
			) SEL'
	end

	if(@ReturnTotalCount = 1) -- Only return count if we requested it
		set @execStmtString=N'with SelNodes(ID, Name, Type, Code, StartDate, EndDate, IsActive, SortOrder, Owner, RowNumber)
			as
			(' + @selectStmtString +
			N'),
			SelNodesCount(TotalCount)
			as
			(
				select count(ID) from SelNodes
			)
			select  TOP ' + cast(@NumRecords as nvarchar(50)) + ' ID, Name, Type, Code, StartDate, EndDate, IsActive, SortOrder, Owner, RowNumber, C.TotalCount as RecordCount
			from SelNodes, SelNodesCount C
			where RowNumber >= ' + cast(@StartingRec as nvarchar(50)) + 
			' order by '+ @OrderClause
	else
		set @execStmtString=N'with SelNodes(ID, Name, Type, Code, StartDate, EndDate, IsActive, SortOrder, Owner, RowNumber)
			as
			(' + @selectStmtString +
			N')
			select  TOP ' + cast(@NumRecords as nvarchar(50)) + ' ID, Name, Type, Code, StartDate, EndDate, IsActive, SortOrder, Owner, RowNumber
			from SelNodes
			where RowNumber >= ' + cast(@StartingRec as nvarchar(50)) +
			' order by '+ @OrderClause
	
	declare @ParamDefinition nvarchar(500)
	set @ParamDefinition = N'@CatalogId int,
						@CatalogNodeId int,
						@StartingRec int,
						@NumRecords int,
						@ReturnInactive bit';
	exec sp_executesql @execStmtString, @ParamDefinition,
			@CatalogId = @CatalogId,
			@CatalogNodeId = @CatalogNodeId,
			@StartingRec = @StartingRec,
			@NumRecords = @NumRecords,
			@ReturnInactive = @ReturnInactive

	SET NOCOUNT OFF
END
GO

-- Update [dbo].[ecf_CatalogNodeSearch]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_CatalogNodeSearch]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_CatalogNodeSearch]
GO
CREATE PROCEDURE [dbo].[ecf_CatalogNodeSearch]
(
	@ApplicationId			uniqueidentifier,
	@SearchSetId			uniqueidentifier,
	@Language 				nvarchar(50),
	@Catalogs 				nvarchar(max),
	@CatalogNodes 			nvarchar(max),
	@SQLClause 				nvarchar(max),
	@MetaSQLClause 			nvarchar(max),
	@OrderBy 				nvarchar(max),
	@Namespace				nvarchar(1024) = N'',
	@Classes				nvarchar(max) = N'',
	@StartingRec 			int,
	@NumRecords   			int,
	@RecordCount			int OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON
	
	DECLARE @query_tmp nvarchar(max)
	DECLARE @FilterQuery_tmp nvarchar(max)
	DECLARE @TableName_tmp sysname
	DECLARE @SelectMetaQuery_tmp nvarchar(max)
	DECLARE @FromQuery_tmp nvarchar(max)
	DECLARE @FullQuery nvarchar(max)

	-- 1. Cycle through all the available catalog node meta classes
	print 'Iterating through meta classes'
	DECLARE MetaClassCursor CURSOR READ_ONLY
	FOR SELECT C.TableName FROM MetaClass C INNER JOIN MetaClass C2 ON C.ParentClassId = C2.MetaClassId
		WHERE C.Namespace like @Namespace + '%' AND (C.[Name] in (select Item from ecf_splitlist(@Classes)) or @Classes = '')
		and C.IsSystem = 0 and C2.[Name] = 'CatalogNode'

		OPEN MetaClassCursor
		FETCH NEXT FROM MetaClassCursor INTO @TableName_tmp
		WHILE (@@fetch_status = 0)
		BEGIN 
			print 'Metaclass Table: ' + @TableName_tmp
			set @Query_tmp = 'select 100 as ''Rank'', META.ObjectId as ''Key'' from ' + @TableName_tmp + ' META'
			
			-- Add meta Where clause
			if(LEN(@MetaSQLClause)>0)
				set @query_tmp = @query_tmp + ' WHERE ' + @MetaSQLClause
			
			if(@SelectMetaQuery_tmp is null)
				set @SelectMetaQuery_tmp = @Query_tmp;
			else
				set @SelectMetaQuery_tmp = @SelectMetaQuery_tmp + N' UNION ALL ' + @Query_tmp;

		FETCH NEXT FROM MetaClassCursor INTO @TableName_tmp
		END
		CLOSE MetaClassCursor
		DEALLOCATE MetaClassCursor

	-- Create from command
	SET @FromQuery_tmp = N'FROM CatalogNode' + N' INNER JOIN (select distinct U.[KEY], U.Rank from (' + @SelectMetaQuery_tmp + N') U) META ON CatalogNode.CatalogNodeId = META.[KEY] '

	set @FromQuery_tmp = @FromQuery_tmp + N' LEFT OUTER JOIN CatalogNodeRelation NR ON CatalogNode.CatalogNodeId = NR.ChildNodeId'
	set @FromQuery_tmp = @FromQuery_tmp + N' LEFT OUTER JOIN [Catalog] CR ON NR.CatalogId = NR.CatalogId'
	set @FromQuery_tmp = @FromQuery_tmp + N' LEFT OUTER JOIN [Catalog] C ON C.CatalogId = CatalogNode.CatalogId'
	set @FromQuery_tmp = @FromQuery_tmp + N' LEFT OUTER JOIN [CatalogNode] CN ON CatalogNode.ParentNodeId = CN.CatalogNodeId'
	set @FromQuery_tmp = @FromQuery_tmp + N' LEFT OUTER JOIN [CatalogNode] CNR ON NR.ParentNodeId = CNR.CatalogNodeId'

	if(Len(@OrderBy) = 0)
	begin
		set @OrderBy = 'CatalogNode.CatalogNodeId'
	end

	/* CATALOG AND NODE FILTERING */
	set @FilterQuery_tmp =  N' WHERE CatalogNode.ApplicationId = ''' + cast(@ApplicationId as nvarchar(100)) + ''' AND ((1=1'
	if(Len(@Catalogs) != 0)
		set @FilterQuery_tmp = @FilterQuery_tmp + N' AND (C.[Name] in (select Item from ecf_splitlist(''' + @Catalogs + ''')))'

	if(Len(@CatalogNodes) != 0)
		set @FilterQuery_tmp = @FilterQuery_tmp + N' AND (CatalogNode.[Code] in (select Item from ecf_splitlist(''' + REPLACE(@CatalogNodes,N'''',N'''''') + '''))))'
	else
		set @FilterQuery_tmp = @FilterQuery_tmp + N')'

	set @FilterQuery_tmp = @FilterQuery_tmp + N' OR (1=1'
	if(Len(@Catalogs) != 0)
		set @FilterQuery_tmp = '' + @FilterQuery_tmp + N' AND (CR.[Name] in (select Item from ecf_splitlist(''' + @Catalogs + ''')))'

	if(Len(@CatalogNodes) != 0)
		set @FilterQuery_tmp = @FilterQuery_tmp + N' AND (CNR.[Code] in (select Item from ecf_splitlist(''' + REPLACE(@CatalogNodes,N'''',N'''''') + '''))))'
	else
		set @FilterQuery_tmp = @FilterQuery_tmp + N')'

	set @FilterQuery_tmp = @FilterQuery_tmp + N')'
	
	-- add sql clause statement here, if specified
	if(Len(@SQLClause) != 0)
		set @FilterQuery_tmp = @FilterQuery_tmp + N' AND (' + @SqlClause + ')'

	set @FullQuery = N'SELECT count(CatalogNode.CatalogNodeId) OVER() TotalRecords, CatalogNode.CatalogNodeId, Rank, ROW_NUMBER() OVER(ORDER BY ' + @OrderBy + N') RowNumber ' + @FromQuery_tmp + @FilterQuery_tmp

	-- use temp table variable
	set @FullQuery = N'with OrderedResults as (' + @FullQuery +') INSERT INTO @Page_temp (TotalRecords, CatalogNodeId) SELECT top(' + cast(@NumRecords as nvarchar(50)) + ') TotalRecords, CatalogNodeId FROM OrderedResults WHERE RowNumber > ' + cast(@StartingRec as nvarchar(50)) + ';'
	set @FullQuery = 'declare @Page_temp table (TotalRecords int, CatalogNodeId int);' + @FullQuery + ';select @RecordCount = TotalRecords from @Page_temp;INSERT INTO CatalogNodeSearchResults (SearchSetId, CatalogNodeId) SELECT ''' + cast(@SearchSetId as nvarchar(100)) + N''', CatalogNodeId from @Page_temp;'
	exec sp_executesql @FullQuery, N'@RecordCount int output', @RecordCount = @RecordCount OUTPUT

	SET NOCOUNT OFF
END
GO

-- Update [dbo].[ecf_OrderSearch]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_OrderSearch]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_OrderSearch]
GO
CREATE PROCEDURE [dbo].[ecf_OrderSearch]
(
	@ApplicationId				uniqueidentifier,
	@SQLClause 					nvarchar(max),
	@MetaSQLClause 				nvarchar(max),
    @OrderBy 					nvarchar(max),
	@Namespace					nvarchar(1024) = N'',
	@Classes					nvarchar(max) = N'',
    @StartingRec 				int,
	@NumRecords   				int,
	@RecordCount                int OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @query_tmp nvarchar(max)
	DECLARE @FilterQuery_tmp nvarchar(max)
	DECLARE @TableName_tmp sysname
	DECLARE @SelectMetaQuery_tmp nvarchar(max)
	DECLARE @FromQuery_tmp nvarchar(max)
	DECLARE @FullQuery nvarchar(max)

	-- 1. Cycle through all the available product meta classes
	print 'Iterating through meta classes'
	DECLARE MetaClassCursor CURSOR READ_ONLY
	FOR SELECT TableName FROM MetaClass 
		WHERE Namespace like @Namespace + '%' AND ([Name] in (select Item from ecf_splitlist(@Classes)) or @Classes = '')
		and IsSystem = 0

	OPEN MetaClassCursor
	FETCH NEXT FROM MetaClassCursor INTO @TableName_tmp
	WHILE (@@fetch_status = 0)
	BEGIN 
		print 'Metaclass Table: ' + @TableName_tmp
		set @Query_tmp = 'select 100 as ''Rank'', META.ObjectId as ''Key'', * from ' + @TableName_tmp + ' META'
		
		-- Add meta Where clause
		if(LEN(@MetaSQLClause)>0)
			set @query_tmp = @query_tmp + ' WHERE ' + @MetaSQLClause

		if(@SelectMetaQuery_tmp is null)
			set @SelectMetaQuery_tmp = @Query_tmp;
		else
			set @SelectMetaQuery_tmp = @SelectMetaQuery_tmp + N' UNION ALL ' + @Query_tmp;

	FETCH NEXT FROM MetaClassCursor INTO @TableName_tmp
	END
	CLOSE MetaClassCursor
	DEALLOCATE MetaClassCursor

	-- Create from command
	SET @FromQuery_tmp = N'FROM [OrderGroup] OrderGroup' + N' INNER JOIN (select distinct U.[KEY], U.Rank from (' + @SelectMetaQuery_tmp + N') U) META ON OrderGroup.[OrderGroupId] = META.[KEY] '

	set @FilterQuery_tmp = N' WHERE ApplicationId = ''' + CAST(@ApplicationId as nvarchar(36)) + ''''
	-- add sql clause statement here, if specified
	if(Len(@SQLClause) != 0)
		set @FilterQuery_tmp = @FilterQuery_tmp + N' AND (' + @SqlClause + ')'

	if(Len(@OrderBy) = 0)
	begin
		set @OrderBy = '[OrderGroup].OrderGroupId'
	end

	set @FullQuery = N'SELECT count([OrderGroup].OrderGroupId) OVER() TotalRecords, [OrderGroup].OrderGroupId, Rank, ROW_NUMBER() OVER(ORDER BY ' + @OrderBy + N') RowNumber ' + @FromQuery_tmp + @FilterQuery_tmp

	-- use temp table variable
	set @FullQuery = N'with OrderedResults as (' + @FullQuery +') INSERT INTO @Page_temp (TotalRecords, OrderGroupId) SELECT top(' + cast(@NumRecords as nvarchar(50)) + ') TotalRecords, OrderGroupId FROM OrderedResults WHERE RowNumber > ' + cast(@StartingRec as nvarchar(50)) + ';'
	set @FullQuery = 'declare @Page_temp table (TotalRecords int, OrderGroupId int);' + @FullQuery + ';select @RecordCount = TotalRecords from @Page_temp;SELECT OrderGroupId from @Page_temp;'
	exec sp_executesql @FullQuery, N'@RecordCount int output', @RecordCount = @RecordCount OUTPUT

	SET NOCOUNT OFF
END
GO

-- Update [dbo].[ecf_Search_PaymentPlan]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_PaymentPlan]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_PaymentPlan]
GO
CREATE PROCEDURE [dbo].[ecf_Search_PaymentPlan]
    @ApplicationId				uniqueidentifier,
	@SQLClause 					nvarchar(max),
	@MetaSQLClause 				nvarchar(max),
    @OrderBy 					nvarchar(max),
	@Namespace					nvarchar(1024) = N'',
	@Classes					nvarchar(max) = N'',
    @StartingRec 				int,
	@NumRecords   				int,
	@RecordCount                int OUTPUT
AS
BEGIN
    declare @results udttOrderGroupId
    insert into @results (OrderGroupId)    
    exec dbo.ecf_OrderSearch
        @ApplicationId, 
        @SQLClause, 
        @MetaSQLClause, 
        @OrderBy, 
        @namespace, 
        @Classes, 
        @StartingRec, 
        @NumRecords, 
        @RecordCount output
	
	exec [dbo].[ecf_Search_OrderGroup] @results

    -- Return Purchase Order Details
	DECLARE @search_condition nvarchar(max)
	CREATE TABLE #OrderSearchResults (OrderGroupId int)
	insert into #OrderSearchResults select OrderGroupId from @results
	SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
	exec mdpsp_avto_OrderGroup_PaymentPlan_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition

	DROP TABLE #OrderSearchResults
END
GO

-- Update [dbo].[ecf_Search_PurchaseOrder]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_PurchaseOrder]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_PurchaseOrder]
GO
CREATE PROCEDURE [dbo].[ecf_Search_PurchaseOrder]
    @ApplicationId				uniqueidentifier,
	@SQLClause 					nvarchar(max),
	@MetaSQLClause 				nvarchar(max),
    @OrderBy 					nvarchar(max),
	@Namespace					nvarchar(1024) = N'',
	@Classes					nvarchar(max) = N'',
    @StartingRec 				int,
	@NumRecords   				int,
	@RecordCount                int OUTPUT
AS
BEGIN
    declare @results udttOrderGroupId
    insert into @results (OrderGroupId)    
    exec dbo.ecf_OrderSearch
        @ApplicationId, 
        @SQLClause, 
        @MetaSQLClause, 
        @OrderBy, 
        @namespace, 
        @Classes, 
        @StartingRec, 
        @NumRecords, 
        @RecordCount output
	
	exec [dbo].[ecf_Search_OrderGroup] @results

    -- Return Purchase Order Details
	DECLARE @search_condition nvarchar(max)
	CREATE TABLE #OrderSearchResults (OrderGroupId int)
	insert into #OrderSearchResults select OrderGroupId from @results
	SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
	exec mdpsp_avto_OrderGroup_PurchaseOrder_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition

	DROP TABLE #OrderSearchResults
END
GO

-- Update [dbo].[ecf_Search_ShoppingCart]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Search_ShoppingCart]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Search_ShoppingCart]
GO
CREATE PROCEDURE [dbo].[ecf_Search_ShoppingCart]
    @ApplicationId				uniqueidentifier,
	@SQLClause 					nvarchar(max),
	@MetaSQLClause 				nvarchar(max),
    @OrderBy 					nvarchar(max),
	@Namespace					nvarchar(1024) = N'',
	@Classes					nvarchar(max) = N'',
    @StartingRec 				int,
	@NumRecords   				int,
	@RecordCount                int OUTPUT
AS
BEGIN
    declare @results udttOrderGroupId
    insert into @results (OrderGroupId)    
    exec dbo.ecf_OrderSearch
        @ApplicationId, 
        @SQLClause, 
        @MetaSQLClause, 
        @OrderBy, 
        @namespace, 
        @Classes, 
        @StartingRec, 
        @NumRecords, 
        @RecordCount output
    
    exec dbo.ecf_Search_OrderGroup @results
    
	IF(EXISTS(SELECT OrderGroupId from OrderGroup where OrderGroupId IN (SELECT [OrderGroupId] FROM @results)))
	begin
	    -- Return Purchase Order Details
		DECLARE @search_condition nvarchar(max)
		CREATE TABLE #OrderSearchResults (OrderGroupId int)
		insert into #OrderSearchResults (OrderGroupId) select OrderGroupId from @results
		SET @search_condition = N'INNER JOIN OrderGroup OG ON OG.OrderGroupId = T.ObjectId WHERE [T].[ObjectId] IN (SELECT [OrderGroupId] FROM #OrderSearchResults)'
		exec mdpsp_avto_OrderGroup_ShoppingCart_Search NULL, '''OrderGroup'' TableName, [OG].*', @search_condition

		DROP TABLE #OrderSearchResults
	end
END
GO

-- Add cludtered to tables which have no indexes
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[Affiliate]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_Affiliate_AffiliateId ON [dbo].[Affiliate] ([AffiliateId]);
GO
if not exists(select * from sys.indexes where object_id = OBJECT_ID('[AzureCompatible]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_AzureCompatible_AzureCompatible ON [dbo].[AzureCompatible] ([AzureCompatible]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[CatalogEntrySearchResults]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_CatalogEntrySearchResults_SearchSetId ON [dbo].[CatalogEntrySearchResults] ([SearchSetId]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[CatalogLanguageMap]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_CatalogLanguageMap_Language ON [dbo].[CatalogLanguageMap] ([Language]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[CatalogNodeSearchResults]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_CatalogNodeSearchResults_SearchSetId ON [dbo].[CatalogNodeSearchResults] ([SearchSetId]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[CatalogSecurity]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_CatalogSecurity_CatalogId ON [dbo].[CatalogSecurity] ([CatalogId]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[McBlobStorage]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_McBlobStorage_McBlobStorage ON [dbo].[McBlobStorage] ([uid]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[SchemaVersion]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_SchemaVersion_Major ON [dbo].[SchemaVersion] ([Major]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[SchemaVersion_ApplicationSystem]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_SchemaVersion_ApplicationSystem_Major ON [dbo].[SchemaVersion_ApplicationSystem] ([Major]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[SchemaVersion_BusinessFoundation]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_SchemaVersion_BusinessFoundation_Major ON [dbo].[SchemaVersion_BusinessFoundation] ([Major]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[SchemaVersion_CatalogSystem]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_SchemaVersion_CatalogSystem_Major ON [dbo].[SchemaVersion_CatalogSystem] ([Major]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[SchemaVersion_MarketingSystem]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_SchemaVersion_MarketingSystem_Major ON [dbo].[SchemaVersion_MarketingSystem] ([Major]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[SchemaVersion_MetaDataSystem]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_SchemaVersion_MetaDataSystem_Major ON [dbo].[SchemaVersion_MetaDataSystem] ([Major]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[SchemaVersion_OrderSystem]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_SchemaVersion_OrderSystem_Major ON [dbo].[SchemaVersion_OrderSystem] ([Major]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[SchemaVersion_ReportingSystem]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_SchemaVersion_ReportingSystem_Major ON [dbo].[SchemaVersion_ReportingSystem] ([Major]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[SchemaVersion_SecuritySystem]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_SchemaVersion_SecuritySystem_Major ON [dbo].[SchemaVersion_SecuritySystem] ([Major]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[SiteCatalog]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_SiteCatalog_CatalogId ON [dbo].[SiteCatalog] ([CatalogId]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[SiteLanguage]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_SiteLanguage_SiteId ON [dbo].[SiteLanguage] ([SiteId]);
GO
if not exists(select 1 from sys.indexes where object_id = OBJECT_ID('[SiteSecurity]') and type_desc = 'CLUSTERED')
	CREATE CLUSTERED INDEX IX_SiteSecurity_SiteId ON [dbo].[SiteSecurity] ([SiteId]);
GO

-- Update stored procedure [dbo].aspnet_Setup_RemoveAllRoleMembers
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[aspnet_Setup_RemoveAllRoleMembers]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) 
	DROP PROCEDURE [dbo].[aspnet_Setup_RemoveAllRoleMembers]
GO
CREATE PROCEDURE [dbo].[aspnet_Setup_RemoveAllRoleMembers]
    @name   sysname
AS
BEGIN
    CREATE TABLE #aspnet_RoleMembers
    (
        Group_name      sysname,
        Group_id        smallint,
        Users_in_group  sysname,
        User_id         smallint
    )
	
	IF (SELECT azureCompatible FROM dbo.AzureCompatible) = 1
		INSERT INTO #aspnet_RoleMembers EXEC sp_helpuser @name
	ELSE
	BEGIN
		INSERT INTO #aspnet_RoleMembers
		select Role_name = substring(r.name, 1, 25), Role_id = r.principal_id,
		   Users_in_role = substring(u.name, 1, 25), Userid = u.principal_id
		from sys.database_principals u, sys.database_principals r, sys.database_role_members m
		where r.name = @name
			and r.principal_id = m.role_principal_id
			and u.principal_id = m.member_principal_id
		order by 1, 2
	END

    DECLARE @user_id smallint
    DECLARE @cmd nvarchar(500)
    DECLARE c1 cursor FORWARD_ONLY FOR
        SELECT User_id FROM #aspnet_RoleMembers

    OPEN c1

    FETCH c1 INTO @user_id
    WHILE (@@fetch_status = 0)
    BEGIN
        SET @cmd = 'EXEC sp_droprolemember ' + '''' + @name + ''', ''' + USER_NAME(@user_id) + ''''
        EXEC (@cmd)
        FETCH c1 INTO @user_id
    END

    CLOSE c1
    DEALLOCATE c1
END
GO

-- Update stored procedure [dbo].[aspnet_Membership_GetNumberOfUsersOnline]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[aspnet_Membership_GetNumberOfUsersOnline]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) 
	DROP PROCEDURE [dbo].[aspnet_Membership_GetNumberOfUsersOnline]
GO
CREATE PROCEDURE dbo.aspnet_Membership_GetNumberOfUsersOnline
    @ApplicationName            nvarchar(256),
    @MinutesSinceLastInActive   int,
    @CurrentTimeUtc             datetime
AS
BEGIN
    DECLARE @DateActive datetime
    SELECT  @DateActive = DATEADD(minute,  -(@MinutesSinceLastInActive), @CurrentTimeUtc)

    DECLARE @NumOnline int
    SELECT  @NumOnline = COUNT(*)
    FROM    dbo.aspnet_Users u WITH (NOLOCK),
            dbo.aspnet_Applications a WITH (NOLOCK),
            dbo.aspnet_Membership m WITH (NOLOCK)
    WHERE   u.ApplicationId = a.ApplicationId                  AND
            LastActivityDate > @DateActive                     AND
            a.LoweredApplicationName = LOWER(@ApplicationName) AND
            u.UserId = m.UserId
    RETURN(@NumOnline)
END
GO

-- Update stored procedure [mdpsp_sys_CreateMetaClass]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_CreateMetaClass]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) 
	DROP PROCEDURE [dbo].[mdpsp_sys_CreateMetaClass]
GO
CREATE PROCEDURE [dbo].[mdpsp_sys_CreateMetaClass]
  @Namespace    NVARCHAR(1024),
  @Name     NVARCHAR(256),
  @FriendlyName   NVARCHAR(256),
  @TableName    NVARCHAR(256),
  @ParentClassId    INT,
  @IsSystem   BIT,
  @IsAbstract   BIT = 0,
  @Description    NTEXT,
  @Retval     INT OUTPUT
AS
BEGIN
  -- Step 0. Prepare
  SET NOCOUNT ON
  SET @Retval = -1

BEGIN TRAN
  -- Step 1. Insert a new record in to the MetaClass table
  INSERT INTO [MetaClass] ([Namespace],[Name], [FriendlyName],[Description], [TableName], [ParentClassId], [PrimaryKeyName], [IsSystem], [IsAbstract])
    VALUES (@Namespace, @Name, @FriendlyName, @Description, @TableName, @ParentClassId, 'undefined', @IsSystem, @IsAbstract)

  IF @@ERROR <> 0 GOTO ERR

  SET @Retval = @@IDENTITY

  declare @tableParameter nvarchar(400)
  
  IF (SELECT azureCompatible FROM dbo.AzureCompatible) = 1
    SET @tableParameter = ''
  ELSE
    SET @tableParameter = ' ON [PRIMARY]'
  
  IF @IsSystem = 1
  BEGIN
    IF NOT EXISTS(SELECT * FROM SYSOBJECTS WHERE [NAME] = @TableName AND [type] = 'U')
    BEGIN
      RAISERROR ('Wrong System TableName.', 16,1 )
      GOTO ERR
    END

    -- Step 3-2. Insert a new record in to the MetaField table
    INSERT INTO [MetaField]  ([Namespace], [Name], [FriendlyName], [SystemMetaClassId], [DataTypeId], [Length], [AllowNulls],  [MultiLanguageValue], [AllowSearch], [IsEncrypted])
       SELECT @Namespace+ N'.' + @Name, SC .[name] , SC .[name] , @Retval ,MDT .[DataTypeId], SC .[length], SC .[isnullable], 0, 0, 0  FROM SYSCOLUMNS AS SC
        INNER JOIN SYSOBJECTS SO ON SO.[ID] = SC.[ID]
        INNER JOIN SYSTYPES ST ON ST.[xtype] = SC.[xtype]
        INNER JOIN MetaDataType MDT ON MDT.[Name] = ST.[name]
      WHERE SO.[ID]  = object_id( @TableName) and OBJECTPROPERTY( SO.[ID], N'IsTable') = 1 and ST.name<>'sysname'
      ORDER BY COLORDER /* Aug 29, 2006 */

    IF @@ERROR<> 0 GOTO ERR

    -- Step 3-2. Insert a new record in to the MetaClassMetaFieldRelation table
    INSERT INTO [MetaClassMetaFieldRelation]  (MetaClassId, MetaFieldId)
      SELECT @Retval, MetaFieldId FROM MetaField WHERE [SystemMetaClassId] = @Retval
  END
  ELSE
  BEGIN
    IF @IsAbstract = 0
    BEGIN
      -- Step 2. Create the @TableName table.
      EXEC('CREATE TABLE [dbo].[' + @TableName  + '] ([ObjectId] [int] NOT NULL , [CreatorId] [nvarchar](100), [Created] [datetime], [ModifierId] [nvarchar](100) , [Modified] [datetime] )' + @tableParameter)

      IF @@ERROR <> 0 GOTO ERR

      EXEC('ALTER TABLE [dbo].[' + @TableName  + '] WITH NOCHECK ADD CONSTRAINT [PK_' + @TableName  + '] PRIMARY KEY  CLUSTERED ([ObjectId])' + @tableParameter)

      IF @@ERROR <> 0 GOTO ERR

      IF EXISTS(SELECT * FROM MetaClass WHERE MetaClassId = @ParentClassId /* AND @IsSystem = 1 */ )
      BEGIN
        -- Step 3-2. Insert a new record in to the MetaClassMetaFieldRelation table
        INSERT INTO [MetaClassMetaFieldRelation]  (MetaClassId, MetaFieldId)
          SELECT @Retval, MetaFieldId FROM MetaField WHERE [SystemMetaClassId] = @ParentClassId
      END

      IF @@ERROR<> 0 GOTO ERR

      -- Step 2-2. Create the @TableName_Localization table
      EXEC('CREATE TABLE [dbo].[' + @TableName + '_Localization] ([Id] [int] IDENTITY (1, 1)  NOT NULL, [ObjectId] [int] NOT NULL , [ModifierId] [nvarchar](100), [Modified] [datetime], [Language] nvarchar(20) NOT NULL)' + @tableParameter)

      IF @@ERROR<> 0 GOTO ERR

      EXEC('ALTER TABLE [dbo].[' + @TableName  + '_Localization] WITH NOCHECK ADD CONSTRAINT [PK_' + @TableName  + '_Localization] PRIMARY KEY  CLUSTERED ([Id])' + @tableParameter)

      IF @@ERROR<> 0 GOTO ERR

      EXEC ('CREATE NONCLUSTERED INDEX IX_' + @TableName + '_Localization_Language ON dbo.' + @TableName + '_Localization ([Language])' + @tableParameter)

      IF @@ERROR<> 0 GOTO ERR

      EXEC ('CREATE UNIQUE NONCLUSTERED INDEX IX_' + @TableName + '_Localization_ObjectId ON dbo.' + @TableName + '_Localization (ObjectId,[Language])' + @tableParameter)

      IF @@ERROR<> 0 GOTO ERR

      declare @system_root_class_id int
      ;with cte as (
        select MetaClassId, ParentClassId, IsSystem
        from MetaClass
        where MetaClassId = @ParentClassId
        union all
        select mc.MetaClassId, mc.ParentClassId, mc.IsSystem
        from cte
        join MetaClass mc on cte.ParentClassId = mc.MetaClassId and cte.IsSystem = 0
      )
      select @system_root_class_id = MetaClassId
      from cte
      where IsSystem = 1

      if exists (select 1 from MetaClass where MetaClassId = @ParentClassId and IsSystem = 1)
      begin
        declare @parent_table sysname
        declare @parent_key_column sysname
        select @parent_table = mc.TableName, @parent_key_column = c.name
        from MetaClass mc
        join sys.key_constraints kc on kc.parent_object_id = OBJECT_ID('[dbo].[' + mc.TableName + ']', 'U')
        join sys.index_columns ic on kc.parent_object_id = ic.object_id and kc.unique_index_id = ic.index_id
        join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
        where mc.MetaClassId = @system_root_class_id
          and kc.type = 'PK'
          and ic.index_column_id = 1
        
        declare @child_table nvarchar(4000)
        declare child_tables cursor local for select @TableName as table_name union all select @TableName + '_Localization'
        open child_tables
        while 1=1
        begin
          fetch next from child_tables into @child_table
          if @@FETCH_STATUS != 0 break
          
          declare @fk_name nvarchar(4000) = 'FK_' + @child_table + '_' + @parent_table
          
          declare @pdeletecascade nvarchar(30) = ' on delete cascade'
          if (@child_table like '%_Localization'
            and @Namespace = 'Mediachase.Commerce.Orders.System')
            begin
            set @pdeletecascade = ''
            end

          declare @fk_sql nvarchar(4000) =
            'alter table [dbo].[' + @child_table + '] add ' +
            case when LEN(@fk_name) <= 128 then 'constraint [' + @fk_name + '] ' else '' end +
            'foreign key (ObjectId) references [dbo].[' + @parent_table + '] ([' + @parent_key_column + '])'+ @pdeletecascade + ' on update cascade'
                        
          execute dbo.sp_executesql @fk_sql
        end
        close child_tables
        
        if @@ERROR != 0 goto ERR
      end

      EXEC mdpsp_sys_CreateMetaClassProcedure @Retval
      IF @@ERROR <> 0 GOTO ERR
    END
  END

  -- Update PK Value
  DECLARE @PrimaryKeyName NVARCHAR(256)
  SELECT @PrimaryKeyName = name FROM Sysobjects WHERE OBJECTPROPERTY(id, N'IsPrimaryKey') = 1 and parent_obj = OBJECT_ID(@TableName) and OBJECTPROPERTY(parent_obj, N'IsUserTable') = 1

  IF @PrimaryKeyName IS NOT NULL
    UPDATE [MetaClass] SET PrimaryKeyName = @PrimaryKeyName WHERE MetaClassId = @Retval

  COMMIT TRAN
RETURN

ERR:
  ROLLBACK TRAN
  SET @Retval = -1
RETURN
END
GO
	
-- Update stored procedure [mdpsp_sys_CreateMetaClassProcedure]
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_CreateMetaClassProcedure]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) 
	DROP PROCEDURE [dbo].[mdpsp_sys_CreateMetaClassProcedure]
GO
CREATE PROCEDURE [dbo].[mdpsp_sys_CreateMetaClassProcedure]
    @MetaClassId int
AS
BEGIN
    set nocount on
    begin try
        declare @CRLF nchar(1) = CHAR(10)
        declare @MetaClassName nvarchar(256)
        declare @TableName sysname
        select @MetaClassName = Name, @TableName = TableName from MetaClass where MetaClassId = @MetaClassId
        if @MetaClassName is null raiserror('Metaclass not found.',16,1)
		
        declare @azureCompatible bit
        SET @azureCompatible = (SELECT TOP 1 azureCompatible FROM dbo.AzureCompatible)
		
        -- get required info for each field
        declare @ParameterIndex int
        declare @ColumnName sysname
        declare @FieldIsMultilanguage bit
		declare @FieldIsEncrypted bit
        declare @FieldIsNullable bit
        declare @ColumnDataType sysname
        declare fields cursor local for
            select
                mfindex.ParameterIndex,
                mf.Name as ColumnName,
                mf.MultiLanguageValue as FieldIsMultilanguage,
				mf.IsEncrypted as FieldIsEncrypted,
                mf.AllowNulls,
                mdt.SqlName + case
                        when mdt.Variable = 1 then '(' + CAST(mf.Length as nvarchar) + ')'
                        when mf.DataTypeId in (5,24) and mfprecis.Value is not null and mfscale.Value is not null then '(' + cast(mfprecis.Value as nvarchar) + ',' + cast(mfscale.Value as nvarchar) + ')'
                        else '' end as ColumnDataType
            from (
                select ROW_NUMBER() over (order by innermf.Name) as ParameterIndex, innermf.MetaFieldId
                from MetaField innermf
                where innermf.SystemMetaClassId = 0
                  and exists (select 1 from MetaClassMetaFieldRelation cfr where cfr.MetaClassId = @MetaClassId and cfr.MetaFieldId = innermf.MetaFieldId)) mfindex
            join MetaField mf on mfindex.MetaFieldId = mf.MetaFieldId
            join MetaDataType mdt on mf.DataTypeId = mdt.DataTypeId
            left outer join MetaAttribute mfprecis on mf.MetaFieldId = mfprecis.AttrOwnerId and mfprecis.AttrOwnerType = 2 and mfprecis.[Key] = 'MdpPrecision'
            left outer join MetaAttribute mfscale on mf.MetaFieldId = mfscale.AttrOwnerId and mfscale.AttrOwnerType = 2 and mfscale.[Key] = 'MdpScale'

        -- aggregate field parts into lists for stored procedures
        declare @ParameterName nvarchar(max)
        declare @ColumnReadBase nvarchar(max)
        declare @ColumnReadLocal nvarchar(max)
        declare @WriteValue nvarchar(max)
        declare @ParameterDefinitions nvarchar(max) = ''
        declare @UnlocalizedSelectValues nvarchar(max) = ''
        declare @LocalizedSelectValues nvarchar(max) = ''
        declare @AllInsertColumns nvarchar(max) = ''
        declare @AllInsertValues nvarchar(max) = ''
        declare @BaseInsertColumns nvarchar(max) = ''
        declare @BaseInsertValues nvarchar(max) = ''
        declare @LocalInsertColumns nvarchar(max) = ''
        declare @LocalInsertValues nvarchar(max) = ''
        declare @AllUpdateActions nvarchar(max) = ''
        declare @BaseUpdateActions nvarchar(max) = ''
        declare @LocalUpdateActions nvarchar(max) = ''
        open fields
        while 1=1
        begin
            fetch next from fields into @ParameterIndex, @ColumnName, @FieldIsMultilanguage, @FieldIsEncrypted, @FieldIsNullable, @ColumnDataType
            if @@FETCH_STATUS != 0 break

            set @ParameterName = '@f' + cast(@ParameterIndex as nvarchar(10))
            set @ColumnReadBase = case when @azureCompatible = 0 and @FieldIsEncrypted = 1 then 'dbo.mdpfn_sys_EncryptDecryptString(T.[' + @ColumnName + '],0)' + ' as [' + @ColumnName + ']' else 'T.[' + @ColumnName + ']' end
            set @ColumnReadLocal = case when @azureCompatible = 0 and @FieldIsEncrypted = 1 then 'dbo.mdpfn_sys_EncryptDecryptString(L.[' + @ColumnName + '],0)' + ' as [' + @ColumnName + ']' else 'L.[' + @ColumnName + ']' end
            set @WriteValue = case when @azureCompatible = 0 and @FieldIsEncrypted = 1 then 'dbo.mdpfn_sys_EncryptDecryptString(' + @ParameterName + ',1)' else @ParameterName end

            set @ParameterDefinitions = @ParameterDefinitions + ',' + @ParameterName + ' ' + @ColumnDataType
            set @UnlocalizedSelectValues = @UnlocalizedSelectValues + ',' + @ColumnReadBase
            set @LocalizedSelectValues = @LocalizedSelectValues + ',' + case when @FieldIsMultilanguage = 1 then @ColumnReadLocal else @ColumnReadBase end
            set @AllInsertColumns = @AllInsertColumns + ',[' + @ColumnName + ']'
            set @AllInsertValues = @AllInsertValues + ',' + @WriteValue
            set @BaseInsertColumns = @BaseInsertColumns + case when @FieldIsMultilanguage = 0 then ',[' + @ColumnName + ']' else '' end
            set @BaseInsertValues = @BaseInsertValues + case when @FieldIsMultilanguage = 0 then ',' + @WriteValue else '' end
            set @LocalInsertColumns = @LocalInsertColumns + case when @FieldIsMultilanguage = 1 then ',[' + @ColumnName + ']' else '' end
            set @LocalInsertValues = @LocalInsertValues + case when @FieldIsMultilanguage = 1 then ',' + @WriteValue else '' end
            set @AllUpdateActions = @AllUpdateActions + ',[' + @ColumnName + ']=' + @WriteValue
            set @BaseUpdateActions = @BaseUpdateActions + ',[' + @ColumnName + ']=' + case when @FieldIsMultilanguage = 0 then @WriteValue when @FieldIsNullable = 1 then 'null' else 'default' end
            set @LocalUpdateActions = @LocalUpdateActions + ',[' + @ColumnName + ']=' + case when @FieldIsMultilanguage = 1 then @WriteValue when @FieldIsNullable = 1 then 'null' else 'default' end
        end
        close fields
        
		declare @OpenEncryptionKey nvarchar(max)
        declare @CloseEncryptionKey nvarchar(max)
        if exists(  select 1
                    from MetaField mf
                    join MetaClassMetaFieldRelation cfr on mf.MetaFieldId = cfr.MetaFieldId
                    where cfr.MetaClassId = @MetaClassId and mf.SystemMetaClassId = 0 and mf.IsEncrypted = 1) and @azureCompatible = 0
        begin
            set @OpenEncryptionKey = 'exec dbo.mdpsp_sys_OpenSymmetricKey' + @CRLF
            set @CloseEncryptionKey = 'exec dbo.mdpsp_sys_CloseSymmetricKey' + @CRLF
        end
        else
        begin
            set @OpenEncryptionKey = ''
            set @CloseEncryptionKey = ''
        end
		
        -- create stored procedures
        declare @procedures table (name sysname, defn nvarchar(max), verb nvarchar(max))

        insert into @procedures (name, defn)
        values ('mdpsp_avto_' + @TableName + '_Get',
            'procedure dbo.[mdpsp_avto_' + @TableName + '_Get] @ObjectId int,@Language nvarchar(20)=null as ' + @CRLF +
            'begin' + @CRLF +
			@OpenEncryptionKey +
            'if @Language is null select T.ObjectId,T.CreatorId,T.Created,T.ModifierId,T.Modified' + @UnlocalizedSelectValues + @CRLF +
            'from [' + @TableName + '] T where ObjectId=@ObjectId' + @CRLF +
            'else select T.ObjectId,T.CreatorId,T.Created,T.ModifierId,T.Modified' + @LocalizedSelectValues + @CRLF +
            'from [' + @TableName + '] T' + @CRLF +
            'left join [' + @TableName + '_Localization] L on T.ObjectId=L.ObjectId and L.Language=@Language' + @CRLF +
            'where T.ObjectId= @ObjectId' + @CRLF +
			@CloseEncryptionKey +
            'end' + @CRLF)

        insert into @procedures (name, defn)
        values ('mdpsp_avto_' + @TableName + '_Update',
            'procedure dbo.[mdpsp_avto_' + @TableName + '_Update]' + @CRLF +
            '@ObjectId int,@Language nvarchar(20)=null,@CreatorId nvarchar(100),@Created datetime,@ModifierId nvarchar(100),@Modified datetime,@Retval int out' + @ParameterDefinitions + ' as' + @CRLF +
            'begin' + @CRLF +
            'set nocount on' + @CRLF +
            'declare @ins bit' + @CRLF +
            'begin try' + @CRLF +
            'begin transaction' + @CRLF +
			@OpenEncryptionKey +
            'if @ObjectId=-1 select @ObjectId=isnull(MAX(ObjectId),0)+1, @Retval=@ObjectId, @ins=0 from [' + @TableName + ']' + @CRLF +
            'else set @ins=case when exists(select 1 from [' + @TableName + '] where ObjectId=@ObjectId) then 0 else 1 end' + @CRLF +
            'if @Language is null' + @CRLF +
            'begin' + @CRLF +
            '  if @ins=1 insert [' + @TableName + '] (ObjectId,CreatorId,Created,ModifierId,Modified' + @AllInsertColumns + ')' + @CRLF +
            '  values (@ObjectId,@CreatorId,@Created,@ModifierId,@Modified' + @AllInsertValues + ')' + @CRLF +
            '  else update [' + @TableName + '] set CreatorId=@CreatorId,Created=@Created,ModifierId=@ModifierId,Modified=@Modified' + @AllUpdateActions + @CRLF +
            '  where ObjectId=@ObjectId' + @CRLF +
            'end' + @CRLF +
            'else' + @CRLF +
            'begin' + @CRLF +
            '  if @ins=1 insert [' + @TableName + '] (ObjectId,CreatorId,Created,ModifierId,Modified' + @BaseInsertColumns + ')' + @CRLF +
            '  values (@ObjectId,@CreatorId,@Created,@ModifierId,@Modified' + @BaseInsertValues + ')' + @CRLF +
            '  else update [' + @TableName + '] set CreatorId=@CreatorId,Created=@Created,ModifierId=@ModifierId,Modified=@Modified' + @BaseUpdateActions + @CRLF +
            '  where ObjectId=@ObjectId' + @CRLF +
            '  if not exists (select 1 from [' + @TableName + '_Localization] where ObjectId=@ObjectId and Language=@Language)' + @CRLF +
            '  insert [' + @TableName + '_Localization] (ObjectId,Language,ModifierId,Modified' + @LocalInsertColumns + ')' + @CRLF +
            '  values (@ObjectId,@Language,@ModifierId,@Modified' + @LocalInsertValues + ')' + @CRLF +
            '  else update [' + @TableName + '_Localization] set ModifierId=@ModifierId,Modified=@Modified' + @LocalUpdateActions + @CRLF +
            '  where ObjectId=@ObjectId and Language=@language' + @CRLF +
            'end' + @CRLF +
			@CloseEncryptionKey +
            'commit transaction' + @CRLF +
            'end try' + @CRLF +
            'begin catch' + @CRLF +
            '  declare @m nvarchar(4000),@v int,@t int' + @CRLF +
            '  select @m=ERROR_MESSAGE(),@v=ERROR_SEVERITY(),@t=ERROR_STATE()' + @CRLF +
            '  rollback transaction' + @CRLF +
            '  raiserror(@m, @v, @t)' + @CRLF +
            'end catch' + @CRLF +
            'end' + @CRLF)

        insert into @procedures (name, defn)
        values ('mdpsp_avto_' + @TableName + '_Delete',
            'procedure dbo.[mdpsp_avto_' + @TableName + '_Delete] @ObjectId int as' + @CRLF +
            'begin' + @CRLF +
            'delete [' + @TableName + '] where ObjectId=@ObjectId' + @CRLF +
            'delete [' + @TableName + '_Localization] where ObjectId=@ObjectId' + @CRLF +
            'exec dbo.mdpsp_sys_DeleteMetaKeyObjects ' + CAST(@MetaClassId as nvarchar(10)) + ',-1,@ObjectId' + @CRLF +
            'end' + @CRLF)

        insert into @procedures (name, defn)
        values ('mdpsp_avto_' + @TableName + '_List',
            'procedure dbo.[mdpsp_avto_' + @TableName + '_List] @Language nvarchar(20)=null,@select_list nvarchar(max)='''',@search_condition nvarchar(max)='''' as' + @CRLF +
            'begin' + @CRLF +
			@OpenEncryptionKey +
            'if @Language is null select T.ObjectId,T.CreatorId,T.Created,T.ModifierId,T.Modified' + @UnlocalizedSelectValues + ' from [' + @TableName + '] T' + @CRLF +
            'else select T.ObjectId,T.CreatorId,T.Created,T.ModifierId,T.Modified' + @LocalizedSelectValues + @CRLF +
            'from [' + @TableName + '] T' + @CRLF +
            'left join [' + @TableName + '_Localization] L on T.ObjectId=L.ObjectId and L.Language=@Language' + @CRLF +
			@CloseEncryptionKey +
            'end' + @CRLF)

        insert into @procedures (name, defn)
        values ('mdpsp_avto_' + @TableName + '_Search',
            'procedure dbo.[mdpsp_avto_' + @TableName + '_Search] @Language nvarchar(20)=null,@select_list nvarchar(max)='''',@search_condition nvarchar(max)='''' as' + @CRLF +
            'begin' + @CRLF +
			'if len(@select_list)>0 set @select_list='',''+@select_list' + @CRLF +
			@OpenEncryptionKey +
            'if @Language is null exec(''select T.ObjectId,T.CreatorId,T.Created,T.ModifierId,T.Modified' + @UnlocalizedSelectValues + '''+@select_list+'' from [' + @TableName + '] T ''+@search_condition)' + @CRLF +
            'else exec(''select T.ObjectId,T.CreatorId,T.Created,T.ModifierId,T.Modified' + @LocalizedSelectValues + '''+@select_list+'' from [' + @TableName + '] T left join [' + @TableName + '_Localization] L on T.ObjectId=L.ObjectId and L.Language=@Language ''+@search_condition)' + @CRLF +
			@CloseEncryptionKey +
            'end' + @CRLF)

        update tgt
        set verb = case when r.ROUTINE_NAME is null then 'create ' else 'alter ' end
        from @procedures tgt
        left outer join INFORMATION_SCHEMA.ROUTINES r on r.ROUTINE_SCHEMA COLLATE DATABASE_DEFAULT = 'dbo' and r.ROUTINE_NAME COLLATE DATABASE_DEFAULT = tgt.name COLLATE DATABASE_DEFAULT

        -- install procedures
        declare @sqlstatement nvarchar(max)
        declare procedure_cursor cursor local for select verb + defn from @procedures
        open procedure_cursor
        while 1=1
        begin
            fetch next from procedure_cursor into @sqlstatement
            if @@FETCH_STATUS != 0 break
            exec(@sqlstatement)
        end
        close procedure_cursor
    end try
    begin catch
        declare @m nvarchar(4000), @v int, @t int
        select @m = ERROR_MESSAGE(), @v = ERROR_SEVERITY(), @t = ERROR_STATE()
        raiserror(@m,@v,@t)
    end catch
END
GO

-- Need to run after updated SP mdpsp_sys_CreateMetaClassProcedure.
EXECUTE dbo.mdpsp_sys_CreateMetaClassProcedureAll
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_CatalogNode_GetChildrenEntries]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_CatalogNode_GetChildrenEntries]
GO

CREATE PROCEDURE [dbo].[ecf_CatalogNode_GetChildrenEntries]
	@CatalogNodeId int
AS
BEGIN
    SELECT ner.CatalogId, ce.MetaClassId, ner.CatalogEntryId, ce.ClassTypeId, ner.CatalogNodeId, ner.SortOrder
	FROM [dbo].NodeEntryRelation ner
	INNER JOIN [dbo].CatalogEntry ce
	ON ner.CatalogEntryId = ce.CatalogEntryId
	WHERE ner.CatalogNodeId = @CatalogNodeId
	ORDER BY ner.SortOrder, ce.Name

END

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_Catalog_GetChildrenEntries]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_Catalog_GetChildrenEntries]
GO

CREATE PROCEDURE [dbo].[ecf_Catalog_GetChildrenEntries]
	@CatalogId int
AS
BEGIN
	SELECT ce.CatalogId, ce.MetaClassId, ce.CatalogEntryId, ClassTypeId, 0 as CatalogNodeId, 0 as SortOrder
	FROM [dbo].CatalogEntry ce
	WHERE ce.CatalogId = @CatalogId
		AND NOT EXISTS(SELECT * FROM NodeEntryRelation R WHERE R.CatalogId = @CatalogId and CE.CatalogEntryId = R.CatalogEntryId)
	ORDER BY ce.Name
END

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_CatalogRelation]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_CatalogRelation]
GO

CREATE PROCEDURE [dbo].[ecf_CatalogRelation]
	@ApplicationId uniqueidentifier,
	@CatalogId int,
	@CatalogNodeId int,
	@CatalogEntryId int,
	@GroupName nvarchar(100),
	@ResponseGroup int
AS
BEGIN
	declare @CatalogNode as int
	declare @CatalogEntry as int
	declare @NodeEntry as int

	set @CatalogNode = 1
	set @CatalogEntry = 2
	set @NodeEntry = 4

	if(@ResponseGroup & @CatalogNode = @CatalogNode)
		SELECT CNR.* FROM CatalogNodeRelation CNR
			INNER JOIN CatalogNode CN ON CN.CatalogNodeId = CNR.ParentNodeId AND (CN.CatalogId = @CatalogId OR @CatalogId = 0)
		WHERE CN.ApplicationId = @ApplicationId AND (@CatalogNodeId = 0 OR CNR.ParentNodeId = @CatalogNodeId) 
		ORDER BY CNR.SortOrder
	else
		select top 0 * from CatalogNodeRelation

	if(@ResponseGroup & @CatalogEntry = @CatalogEntry)
		SELECT CER.* FROM CatalogEntryRelation CER
			INNER JOIN CatalogEntry CE ON CE.CatalogEntryId = CER.ParentEntryId AND (CE.CatalogId = @CatalogId OR @CatalogId = 0)
		WHERE
			CE.ApplicationId = @ApplicationId AND
			(CER.ParentEntryId = @CatalogEntryId OR @CatalogEntryId = 0) AND
			(CER.GroupName = @GroupName OR LEN(@GroupName) = 0)
		ORDER BY CER.SortOrder
	else
		select top 0 * from CatalogEntryRelation

	if(@ResponseGroup & @NodeEntry = @NodeEntry)
	BEGIN
		declare @execStmt nvarchar(1000)
		set @execStmt = 'SELECT NER.CatalogId, NER.CatalogEntryId, NER.CatalogNodeId, NER.SortOrder FROM NodeEntryRelation NER
			INNER JOIN [Catalog] C ON C.CatalogId = NER.CatalogId
		WHERE 
			C.ApplicationId = @ApplicationId '
		
		if @CatalogId!=0
			set @execStmt = @execStmt + ' AND (NER.CatalogId = @CatalogId) '
		if @CatalogNodeId!=0
			set @execStmt = @execStmt + ' AND (NER.CatalogNodeId = @CatalogNodeId) '
		if @CatalogEntryId!=0
			set @execStmt = @execStmt + ' AND (NER.CatalogEntryId = @CatalogEntryId) '

		set @execStmt = @execStmt + ' ORDER BY NER.SortOrder'
		
		declare @pars nvarchar(500)
		set @pars = '@ApplicationId uniqueidentifier, @CatalogId int, @CatalogNodeId int, @CatalogEntryId int'
		exec sp_executesql @execStmt, @pars,
			@ApplicationId=@ApplicationId, @CatalogId=@CatalogId, @CatalogNodeId=@CatalogNodeId, @CatalogEntryId=@CatalogEntryId
	END
	else
		select top 0 CatalogId, CatalogEntryId, CatalogNodeId, SortOrder from NodeEntryRelation
END
GO 

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdptr_sys_MetaField_IsKeyField]') AND OBJECTPROPERTY(id, N'IsTrigger') = 1) DROP TRIGGER [dbo].[mdptr_sys_MetaField_IsKeyField]
GO 

create trigger dbo.mdptr_sys_MetaField_IsKeyField
on MetaField after insert, update
as
begin
    set nocount on
    if update(SystemMetaClassId)
    begin
        update dst
		set IsKeyField = cast(case when exists(
                select 1
	            from MetaClass mc
	            join INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu on kcu.CONSTRAINT_NAME COLLATE DATABASE_DEFAULT = mc.PrimaryKeyName COLLATE DATABASE_DEFAULT and kcu.CONSTRAINT_SCHEMA COLLATE DATABASE_DEFAULT = 'dbo'
	            where mc.MetaClassId = dst.SystemMetaClassId
	              and kcu.COLUMN_NAME COLLATE DATABASE_DEFAULT = dst.Name COLLATE DATABASE_DEFAULT)
	        then 1 else 0 end as bit)
		from MetaField dst
        where dst.MetaFieldId in (select i.MetaFieldId from inserted i)
        -- do not check for actual value change. updates to MetaClass.PrimaryKeyName will fire this
		-- trigger with "update MetaField set SystemMetaClassId=SystemMetaClassId".
    end
end 

GO 


--beginUpdatingDatabaseVersion 
 
INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(6, 0, 0, GETUTCDATE()) 
GO 

--endUpdatingDatabaseVersion 
