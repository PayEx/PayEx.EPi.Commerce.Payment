--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 6, @minor int = 3, @patch int = 1   
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
 
GO 



IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_OrderGroupNote_ListByOrderGroupIds]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_OrderGroupNote_ListByOrderGroupIds] 

GO 

CREATE PROCEDURE [dbo].[ecf_OrderGroupNote_ListByOrderGroupIds]
	@OrderGroupIds udttOrderGroupId readonly
AS
BEGIN
SET NOCOUNT ON;

SELECT [t01].OrderNoteId, 
	[t01].CustomerId, 
	[t01].Created, 
	[t01].OrderGroupId, 
	[t01].Detail,
	[t01].LineItemId,
	[t01].Title,
	[t01].Type 
FROM [OrderGroupNote] AS [t01]
INNER JOIN @OrderGroupIds r
ON ([t01].OrderGroupId = r.OrderGroupId)

END

GO 


-- Update ecf_OrderForm_Delete SP to fix the error when deleting exchange order
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_OrderForm_Delete]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_OrderForm_Delete] 
GO 

CREATE PROCEDURE [dbo].[ecf_OrderForm_Delete]
(
	@OrderFormId int
)
AS
	SET NOCOUNT ON
	DECLARE @TempObjectId int	

	-- Delete line items
	DECLARE _cursor CURSOR READ_ONLY FOR 
		SELECT LineItemId FROM [LineItem] where OrderFormId = @OrderFormId
	OPEN _cursor
	FETCH NEXT FROM _cursor INTO @TempObjectId
	WHILE (@@fetch_status = 0) BEGIN
		DELETE FROM [LineItemDiscount] where LineItemId = @TempObjectId
		EXEC [dbo].[mdpsp_avto_LineItemEx_Delete] @TempObjectId
		FETCH NEXT FROM _cursor INTO @TempObjectId
	END
	CLOSE _cursor
	DEALLOCATE _cursor
	DELETE FROM [LineItem] where OrderFormId = @OrderFormId

	-- Delete payments
	DECLARE _cursor CURSOR READ_ONLY FOR 
		SELECT PaymentId FROM [OrderFormPayment] where OrderFormId = @OrderFormId
	OPEN _cursor
	FETCH NEXT FROM _cursor INTO @TempObjectId
	WHILE (@@fetch_status = 0) BEGIN
		EXEC [dbo].[mdpsp_avto_OrderFormPayment_CashCard_Delete] @TempObjectId
		EXEC [dbo].[mdpsp_avto_OrderFormPayment_CreditCard_Delete] @TempObjectId
		EXEC [dbo].[mdpsp_avto_OrderFormPayment_GiftCard_Delete] @TempObjectId	
		EXEC [dbo].[mdpsp_avto_OrderFormPayment_Invoice_Delete] @TempObjectId
		EXEC [dbo].[mdpsp_avto_OrderFormPayment_Other_Delete] @TempObjectId
		
		EXEC [dbo].[mdpsp_avto_OrderFormPayment_Exchange_Delete] @TempObjectId
		FETCH NEXT FROM _cursor INTO @TempObjectId
	END
	CLOSE _cursor
	DEALLOCATE _cursor
	DELETE FROM [OrderFormPayment] where OrderFormId = @OrderFormId

	-- Delete OrderFormDiscount
	DELETE FROM [OrderFormDiscount] where OrderFormId = @OrderFormId

	-- Delete Shipment
	DECLARE _cursor CURSOR READ_ONLY FOR 
		SELECT ShipmentId FROM [Shipment] where OrderFormId = @OrderFormId
	OPEN _cursor
	FETCH NEXT FROM _cursor INTO @TempObjectId
	WHILE (@@fetch_status = 0) BEGIN
		DELETE FROM [ShipmentDiscount] where ShipmentId = @TempObjectId
		EXEC [dbo].[mdpsp_avto_ShipmentEx_Delete] @TempObjectId
		FETCH NEXT FROM _cursor INTO @TempObjectId
	END
	CLOSE _cursor
	DEALLOCATE _cursor
	DELETE FROM [Shipment] where OrderFormId = @OrderFormId

	-- Delete OrderForm
	select @TempObjectId = OrderFormId FROM [OrderForm] where OrderFormId = @OrderFormId
	EXEC [dbo].[mdpsp_avto_OrderFormEx_Delete] @TempObjectId
	DELETE FROM [OrderForm] where OrderFormId = @OrderFormId

	RETURN @@Error
GO
 
--beginUpdatingDatabaseVersion 
 
INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(6, 3, 1, GETUTCDATE()) 
 
GO 

--endUpdatingDatabaseVersion 
