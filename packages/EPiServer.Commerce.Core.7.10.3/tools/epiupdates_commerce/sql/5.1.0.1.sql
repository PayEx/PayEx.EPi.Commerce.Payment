--beginvalidatingquery
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion')
    BEGIN
    declare @major int = 5, @minor int = 1, @patch int = 0
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch)
        select 0,'Already correct database version'
    ELSE
        select 1, 'Upgrading database'
    END
ELSE
    select -1, 'Not an EPiServer Commerce database'
--endvalidatingquery

GO

ALTER PROCEDURE [dbo].[GetContentSchemaVersionNumber]
AS
	DECLARE @major int, @minor int, @patch int

	SELECT @major = max([Major]) FROM [SchemaVersion]
	SELECT @minor = max([Minor]) FROM [SchemaVersion] WHERE Major = @major
	SELECT @patch = max([Patch]) FROM [SchemaVersion] WHERE Major = @major AND Minor = @minor

	SELECT Major, Minor, Patch, InstallDate FROM [SchemaVersion]
	WHERE
		Major = @major AND
		Minor = @minor AND
		Patch = @patch

GO

ALTER TABLE [dbo].[ShippingMethodParameter] ALTER COLUMN [Value] [nvarchar](512) NULL
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_OrderGroupNote_ListByOrderGroupId]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_OrderGroupNote_ListByOrderGroupId]
GO

CREATE PROCEDURE [dbo].[ecf_OrderGroupNote_ListByOrderGroupId]
@OrderGroupId AS Int
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
WHERE ([t01].OrderGroupId=@OrderGroupId)

END
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_OrderGroupLock_GetByOrderGroupId]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_OrderGroupLock_GetByOrderGroupId]
GO

CREATE PROCEDURE [dbo].[ecf_OrderGroupLock_GetByOrderGroupId]
@OrderGroupId AS Int
AS
BEGIN
SET NOCOUNT ON;

SELECT top 1 [t01].[OrderLockId] AS [OrderLockId], [t01].[CustomerId] AS [CustomerId], [t01].[Created] AS [Created], [t01].[OrderGroupId] AS [OrderGroupId]
FROM [OrderGroupLock] AS [t01]
WHERE ([t01].[OrderGroupId]=@OrderGroupId)

END
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_OrderGroupLock_ListByCustomerId]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_OrderGroupLock_ListByCustomerId]
GO

CREATE PROCEDURE [dbo].[ecf_OrderGroupLock_ListByCustomerId]
@CustomerId AS Uniqueidentifier
AS
BEGIN
SET NOCOUNT ON;

SELECT [t01].[OrderLockId] AS [OrderLockId], [t01].[CustomerId] AS [CustomerId], [t01].[Created] AS [Created], [t01].[OrderGroupId] AS [OrderGroupId]
FROM [OrderGroupLock] AS [t01]
WHERE ([t01].[CustomerId]=@CustomerId)

END
GO


IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_mktg_PromotionByDate]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_mktg_PromotionByDate]
GO

CREATE PROCEDURE [dbo].[ecf_mktg_PromotionByDate]
    @ApplicationId uniqueidentifier,
    @DateTime datetime
AS
BEGIN
    SELECT P.* from [Promotion] P
    INNER JOIN Campaign C ON C.CampaignId = P.CampaignId
    WHERE
        P.ApplicationId = @ApplicationId and
        (@DateTime between P.StartDate and DATEADD(week, 1, P.EndDate)) and
        P.Status = 'active' and
        (@DateTime between C.StartDate and DATEADD(week, 1, C.EndDate)) and
        C.IsActive = 1
    ORDER BY
        P.Priority  DESC, P.CouponCode DESC, P.PromotionGroup

    SELECT PC.* from [PromotionCondition] PC
    INNER JOIN [Promotion] P ON P.PromotionId = PC.PromotionId
    INNER JOIN Campaign C ON C.CampaignId = P.CampaignId
    WHERE
        P.ApplicationId = @ApplicationId and
        (@DateTime between P.StartDate and DATEADD(week, 1, P.EndDate)) and
        P.Status = 'active' and
        (@DateTime between C.StartDate and DATEADD(week, 1, C.EndDate)) and
        C.IsActive = 1

    SELECT PG.* from [PromotionLanguage] PG
    INNER JOIN [Promotion] P ON P.PromotionId = PG.PromotionId
    INNER JOIN Campaign C ON C.CampaignId = P.CampaignId
    WHERE
        P.ApplicationId = @ApplicationId and
        (@DateTime between P.StartDate and DATEADD(week, 1, P.EndDate)) and
        P.Status = 'active' and
        (@DateTime between C.StartDate and DATEADD(week, 1, C.EndDate)) and
        C.IsActive = 1

    SELECT PP.* from [PromotionPolicy] PP
    INNER JOIN [Promotion] P ON P.PromotionId = PP.PromotionId
    INNER JOIN Campaign C ON C.CampaignId = P.CampaignId
    WHERE
        P.ApplicationId = @ApplicationId and
        (@DateTime between P.StartDate and DATEADD(week, 1, P.EndDate)) and
        P.Status = 'active' and
        (@DateTime between C.StartDate and DATEADD(week, 1, C.EndDate)) and
        C.IsActive = 1
END
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_mktg_Promotion]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_mktg_Promotion]
GO

CREATE PROCEDURE [dbo].[ecf_mktg_Promotion]
    @ApplicationId uniqueidentifier,
    @PromotionId int
AS
BEGIN

    if(@PromotionId = 0)
        set @PromotionId = null

    SELECT P.* from [Promotion] P
    WHERE
        P.ApplicationId = @ApplicationId and
        P.PromotionId = COALESCE(@PromotionId,P.PromotionId)
    ORDER BY
        P.Priority  DESC, P.CouponCode DESC, P.PromotionGroup

    SELECT PC.* from [PromotionCondition] PC
    INNER JOIN [Promotion] P ON P.PromotionId = PC.PromotionId
    WHERE
        P.ApplicationId = @ApplicationId and
        PC.PromotionId = COALESCE(@PromotionId,PC.PromotionId)

    SELECT PG.* from [PromotionLanguage] PG
    INNER JOIN [Promotion] P ON P.PromotionId = PG.PromotionId
    WHERE
        P.ApplicationId = @ApplicationId and
        PG.PromotionId = COALESCE(@PromotionId,PG.PromotionId)

    SELECT PP.* from [PromotionPolicy] PP
    INNER JOIN [Promotion] P ON P.PromotionId = PP.PromotionId
    WHERE
        P.ApplicationId = @ApplicationId and
        PP.PromotionId = COALESCE(@PromotionId,PP.PromotionId)

END
GO

IF EXISTS (SELECT * FROM Information_schema.Routines WHERE Specific_schema = 'dbo' AND specific_name = 'fn_GetDaylightSavingsTime' AND Routine_Type = 'FUNCTION' ) DROP FUNCTION [dbo].[fn_GetDaylightSavingsTime]
GO

CREATE FUNCTION [dbo].[fn_GetDaylightSavingsTime]
(
 @UTC_Date DATETIME,
 @ST_Offset INT, -- CST = -6, EST = -5
 @DT_Offset INT  -- CDT = -5, EDT = -4
)
RETURNS DATETIME
AS
BEGIN
RETURN
DATEADD(hh,
   CASE WHEN YEAR(@UTC_Date) <= 2006 THEN
                CASE WHEN
                      @UTC_Date >=  '4/' + CAST(ABS(8 - DATEPART(WEEKDAY,'4/1/'
    + CAST(YEAR(@UTC_Date) AS VARCHAR))) % 7 + 1 AS VARCHAR) +  '/'
    + CAST(YEAR(@UTC_Date) AS VARCHAR) + ' 2:00' AND
                      @UTC_Date < '10/' + CAST(32 - DATEPART(WEEKDAY,'10/31/'
    + CAST(YEAR(@UTC_Date) AS VARCHAR)) AS VARCHAR) +  '/'
    + CAST(YEAR(@UTC_Date) AS VARCHAR) + ' 2:00'
                THEN @DT_Offset ELSE @ST_Offset END
              ELSE
                CASE WHEN
                      @UTC_Date >= '3/' + CAST(ABS(8 - DATEPART(WEEKDAY,'3/1/'
    + CAST(YEAR(@UTC_Date) AS VARCHAR))) % 7 + 8 AS VARCHAR) +  '/'
    + CAST(YEAR(@UTC_Date) AS VARCHAR) + ' 2:00' AND
                      @UTC_Date <
                        '11/' + CAST(ABS(8 - DATEPART(WEEKDAY,'11/1/'
    + CAST(YEAR(@UTC_Date) AS VARCHAR))) % 7 + 1 AS VARCHAR) +  '/'
    + CAST(YEAR(@UTC_Date) AS VARCHAR) + ' 2:00'
                THEN @DT_Offset ELSE @ST_Offset END
              END
   , @UTC_Date
   )
END
GO

ALTER PROCEDURE [dbo].[ecf_reporting_SaleReport]
	@ApplicationID uniqueidentifier,
	@MarketId nvarchar(8),
	@CurrencyCode NVARCHAR(8),
	@interval VARCHAR(20),
	@startdate DATETIME, -- parameter expected in UTC
	@enddate DATETIME, -- parameter expected in UTC
	@offset_st INT,
	@offset_dt INT
AS
BEGIN
	with periodQuery as
	(
		SELECT DISTINCT
			(CASE WHEN @interval = 'Day'
				THEN CONVERT(VARCHAR(10), D.Datefull, 101)
			WHEN @interval = 'Month'
			THEN (DATENAME(MM, D.Datefull) + ',' + CAST(YEAR(D.Datefull) AS VARCHAR(20)))
			ElSE CAST(YEAR(D.Datefull) AS VARCHAR(20))
			End) AS Period
		FROM ReportingDates D
		WHERE
			-- convert back from UTC using offset to generate a list of WEBSERVER datetimes
			D.Datefull BETWEEN
				cast(floor(cast(dbo.fn_GetDaylightSavingsTime(@startdate, @offset_st, @offset_dt) as float)) as datetime) AND
				cast(floor(cast(dbo.fn_GetDaylightSavingsTime(@enddate, @offset_st, @offset_dt) as float)) as datetime)
	)
	, lineItemsQuery as
	(
		select sum(Quantity) ItemsOrdered, L.OrderGroupId
		from LineItem L
			inner join OrderForm as OF1 on L.OrderFormId = OF1.OrderFormId
		where OF1.Name <> 'Return'
		group by L.OrderGroupId
	)
	, orderFormQuery as
	(
		select sum(DiscountAmount) Discounts, OrderGroupId
		from OrderForm
		group by OrderGroupId
	)
	, paymentQuery as
	(
		select sum(Amount) TotalPayment, OFP.OrderGroupId
		from OrderFormPayment as OFP
		where OFP.TransactionType = 'Capture' OR OFP.TransactionType = 'Sale'
		group by OFP.OrderGroupId
	)
	, orderQuery as
	(
		SELECT
			(CASE WHEN @interval = 'Day'
				THEN CONVERT(VARCHAR(10), dbo.fn_GetDaylightSavingsTime(PO.Created, @offset_st, @offset_dt), 101)
				WHEN @interval = 'Month'
				THEN (DATENAME(MM, dbo.fn_GetDaylightSavingsTime(PO.Created, @offset_st, @offset_dt)) + ','
					+ CAST(YEAR(dbo.fn_GetDaylightSavingsTime(PO.Created, @offset_st, @offset_dt)) AS VARCHAR(20)))
				ElSE CAST(YEAR(dbo.fn_GetDaylightSavingsTime(PO.Created, @offset_st, @offset_dt)) AS VARCHAR(20))
				End) AS Period,
			COALESCE(COUNT(OG.OrderGroupId), 0) AS NumberofOrders
			, SUM(L1.ItemsOrdered) AS ItemsOrdered
			, SUM(OG.SubTotal) AS SubTotal
			, SUM(OG.TaxTotal) AS Tax
			, SUM(OG.ShippingTotal) AS Shipping
			, SUM(OF1.Discounts) AS Discounts
			, SUM(OG.Total) AS Total
			, SUM(P.TotalPayment) AS Invoiced
		FROM OrderGroup AS OG
			INNER JOIN OrderGroup_PurchaseOrder AS PO
				ON PO.ObjectId = OG.OrderGroupId
			INNER JOIN orderFormQuery OF1
				on OF1.OrderGroupId = OG.OrderGroupId
			LEFT JOIN paymentQuery AS P
				ON P.OrderGroupId = OG.OrderGroupId
			LEFT JOIN lineItemsQuery L1
				on L1.OrderGroupId = OG.OrderGroupId
        WHERE
			-- PO.Created is stored in UTC
            PO.Created
			BETWEEN
				-- pad range by one day to include outlying records on narrow date ranges
				DATEADD(DD, -1, @startdate) AND
				DATEADD(DD, 1, @enddate)
			AND OG.Name <> 'Exchange'
			AND OG.BillingCurrency = @CurrencyCode
			AND (LEN(@MarketId) = 0 OR OG.MarketId = @MarketId)
		GROUP BY
			(CASE WHEN @interval = 'Day'
				THEN CONVERT(VARCHAR(10), dbo.fn_GetDaylightSavingsTime(PO.Created, @offset_st, @offset_dt), 101)
				WHEN @interval = 'Month'
				THEN (DATENAME(MM, dbo.fn_GetDaylightSavingsTime(PO.Created, @offset_st, @offset_dt)) + ','
					+ CAST(YEAR(dbo.fn_GetDaylightSavingsTime(PO.Created, @offset_st, @offset_dt)) AS VARCHAR(20)))
				ElSE CAST(YEAR(dbo.fn_GetDaylightSavingsTime(PO.Created, @offset_st, @offset_dt)) AS VARCHAR(20))
				End)
	)

	SELECT
		P.Period
		, O.NumberofOrders as NumberofOrders
		, O.ItemsOrdered
		, O.Subtotal
		, O.Tax
		, O.Shipping
		, O.Discounts
		, O.Total
		, O.Invoiced
	FROM periodQuery P LEFT JOIN orderQuery O
		on P.Period = O.Period
	ORDER BY CONVERT(datetime, P.Period, 101)
END
GO

ALTER PROCEDURE [dbo].[ecf_reporting_ProductBestSellers]
	@ApplicationID uniqueidentifier,
	@MarketId nvarchar(8),
	@CurrencyCode NVARCHAR(8),
	@interval VARCHAR(20),
	@startdate DATETIME, -- parameter expected in UTC
	@enddate DATETIME, -- parameter expected in UTC
	@offset_st INT,
	@offset_dt INT
AS
BEGIN
	SELECT	z.Period,
			z.ProductName,
			z.Price,
			z.Ordered,
			z.Code
	FROM
	(
		SELECT	x.Period as Period,
				ISNULL(y.ProductName, 'NONE') AS ProductName,
				ISNULL(y.Price,0) AS Price,
				ISNULL(y.ItemsOrdered, 0) AS Ordered,
				RANK() OVER (PARTITION BY x.Period
						ORDER BY y.price DESC) AS PriceRank,
				y.Code
		FROM
		(
			SELECT	DISTINCT (CASE WHEN @interval = 'Day'
								THEN CONVERT(VARCHAR(10), D.DateFull, 101)
								WHEN @interval = 'Month'
								THEN (DATENAME(MM, D.Datefull) + ', ' + CAST(YEAR(D.Datefull) AS VARCHAR(20)))
								ElSE CAST(YEAR(D.Datefull) AS VARCHAR(20))
								End) AS Period
			FROM ReportingDates D LEFT OUTER JOIN OrderFormEx FEX ON D.Datefull = FEX.Created
		WHERE
			-- convert back from UTC using offset to generate a list of WEBSERVER datetimes
			D.Datefull BETWEEN
				cast(floor(cast(dbo.fn_GetDaylightSavingsTime(@startdate, @offset_st, @offset_dt) as float)) as datetime) AND
				cast(floor(cast(dbo.fn_GetDaylightSavingsTime(@enddate, @offset_st, @offset_dt) as float)) as datetime)
		) AS x
		LEFT JOIN
		(
			SELECT  DISTINCT (CASE WHEN @interval = 'Day'
								THEN CONVERT(VARCHAR(20), dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt), 101)
								WHEN @interval = 'Month'
								THEN (DATENAME(MM, dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) + ', ' + CAST(YEAR(dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) AS VARCHAR(20)) )
								ElSE CAST(YEAR(dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) AS VARCHAR(20))   End) as period,

				 E.Name AS ProductName,
					L.ListPrice AS Price,
					SUM(L.Quantity) AS ItemsOrdered,
					RANK() OVER (PARTITION BY (CASE WHEN @interval = 'Day'
													THEN CONVERT(VARCHAR(20), dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt), 101)
													WHEN @interval = 'Month'
													THEN (DATENAME(MM, dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) + ', ' + CAST(YEAR(dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) AS VARCHAR(20)) )
													ElSE CAST(YEAR(dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) AS VARCHAR(20))
												END)
								ORDER BY SUM(L.Quantity) DESC) AS PeriodRank,
					E.Code
			FROM
				LineItem AS L INNER JOIN OrderFormEx AS FEX ON L.OrderFormId = Fex.ObjectId
				INNER JOIN OrderForm AS F ON L.OrderFormID = F.OrderFormID
				INNER JOIN CatalogEntry E ON L.CatalogEntryID = E.Code
				INNER JOIN OrderGroup AS OG ON F.OrderGroupId = OG.OrderGroupId AND isnull (OG.Status, '') = 'Completed'
			WHERE CONVERT(VARCHAR(20), dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt), 101) >=  @startdate AND CONVERT(VARCHAR(20), dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt), 101) < @enddate +1
				AND @applicationid = (SELECT  ApplicationID FROM OrderGroup  WHERE OrderGroupID = F.OrderGroupID)
				AND (FEX.RMANumber = '' OR FEX.RMANumber IS NULL)
				AND OG.Name <> 'Exchange'
				AND OG.BillingCurrency = @CurrencyCode
				AND (LEN(@MarketId) = 0 OR OG.MarketId = @MarketId)
			GROUP BY (Case WHEN @interval = 'Day'
						THEN CONVERT(VARCHAR(20), dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt), 101)
						WHEN @interval = 'Month'
						THEN (DATENAME(MM, dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) + ', ' + CAST(YEAR(dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) AS VARCHAR(20))  )
						ElSE CAST(YEAR(dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) AS VARCHAR(20))
					END) ,E.Name, L.ListPrice, E.Code
		) AS y
ON x.Period = y.Period
WHERE y.PeriodRank IS NULL
OR y.PeriodRank = 1
	)AS z
WHERE z.PriceRank = 1
ORDER BY CONVERT(datetime, z.Period, 101)
END
GO

ALTER PROCEDURE [dbo].[ecf_reporting_Shipping]
	@ApplicationID uniqueidentifier,
	@MarketId nvarchar(8),
	@CurrencyCode NVARCHAR(8),
	@interval VARCHAR(20),
	@startdate DATETIME, -- parameter expected in UTC
	@enddate DATETIME, -- parameter expected in UTC
	@offset_st INT,
	@offset_dt INT
AS
BEGIN
	SELECT	x.Period,
			ISNULL(y.ShippingMethodDisplayName, 'NONE') AS ShippingMethodDisplayName,
			ISNULL(y.NumberofOrders, 0) AS NumberOfOrders,
			ISNULL(y.Shipping, 0) AS TotalShipping

	FROM
	(
		SELECT DISTINCT
			(CASE WHEN @interval = 'Day'
				THEN CONVERT(VARCHAR(10), D.Datefull, 101)
			WHEN @interval = 'Month'
			THEN (DATENAME(MM, D.Datefull) + ', ' + CAST(YEAR(D.Datefull) AS VARCHAR(20)))
			ElSE CAST(YEAR(D.Datefull) AS VARCHAR(20))
			End) AS Period
		FROM ReportingDates D LEFT OUTER JOIN OrderFormEx FEX ON D.Datefull = FEX.Created
		WHERE
			-- convert back from UTC using offset to generate a list of WEBSERVER datetimes
			D.Datefull BETWEEN
				cast(floor(cast(dbo.fn_GetDaylightSavingsTime(@startdate, @offset_st, @offset_dt) as float)) as datetime) AND
				cast(floor(cast(dbo.fn_GetDaylightSavingsTime(@enddate, @offset_st, @offset_dt) as float)) as datetime)
	) AS x
	LEFT JOIN
	(
		SELECT DISTINCT (CASE WHEN @interval = 'Day'
							THEN CONVERT(VARCHAR(20), dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt), 101)
							WHEN @interval = 'Month'
							THEN (DATENAME(MM, dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) + ', '
								+ CAST(YEAR(dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) AS VARCHAR(20)) )
							ElSE CAST(YEAR(dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) AS VARCHAR(20))
							End) AS Period,
				COUNT(S.ShipmentID) AS NumberofOrders,
				SUM(S.ShipmentTotal) AS Shipping,
				SM.DisplayName AS ShippingMethodDisplayName
		FROM Shipment AS S INNER JOIN
		ShippingMethod AS SM ON S.ShippingMethodId = SM.ShippingMethodId INNER JOIN
			OrderForm AS F ON S.OrderFormId = F.OrderFormId INNER JOIN
			OrderFormEx AS FEX ON FEX.ObjectId = F.OrderFormId INNER JOIN
			OrderGroup AS OG ON OG.OrderGroupId = F.OrderGroupId
		WHERE (FEX.Created BETWEEN @startdate AND @enddate)
		AND @applicationid = (SELECT  ApplicationID FROM OrderGroup  WHERE OrderGroupID = F.OrderGroupID)
		AND OG.BillingCurrency = @CurrencyCode
		AND (LEN(@MarketId) = 0 OR OG.MarketId = @MarketId)
		AND S.Status <> 'Cancelled'
		GROUP BY (Case WHEN @interval = 'Day'
					THEN CONVERT(VARCHAR(20), dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt), 101)
					WHEN @interval = 'Month'
					THEN (DATENAME(MM, dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) + ', ' + CAST(YEAR(dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) AS VARCHAR(20))  )
					ElSE CAST(YEAR(dbo.fn_GetDaylightSavingsTime(FEX.Created, @offset_st, @offset_dt)) AS VARCHAR(20))
				END), SM.DisplayName
	) AS y
	ON x.Period = y.Period
	ORDER BY CONVERT(datetime, x.Period, 101)
END
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_CatalogEntryItemSeo_List]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_CatalogEntryItemSeo_List]
GO

CREATE PROCEDURE dbo.ecf_CatalogEntryItemSeo_List
    @CatalogEntries dbo.udttEntityList readonly
AS
BEGIN
    SELECT n.*
    FROM CatalogEntry n
    JOIN @CatalogEntries r ON n.CatalogEntryId = r.EntityId
    ORDER BY r.SortOrder

    SELECT s.*
    FROM CatalogItemSeo s
    JOIN @CatalogEntries r on s.CatalogEntryId = r.EntityId

END
GO

ALTER TABLE [dbo].[CatalogItemAsset] DROP CONSTRAINT [PK_CatalogItemAsset]
ALTER TABLE [dbo].[CatalogItemAsset] ALTER COLUMN [AssetType] NVARCHAR(190) NOT NULL
ALTER TABLE [dbo].[CatalogItemAsset] ADD CONSTRAINT [PK_CatalogItemAsset] PRIMARY KEY CLUSTERED ([CatalogNodeId] ASC, [CatalogEntryId] ASC, [AssetType] ASC, [AssetKey] ASC)
GO

--beginUpdatingDatabaseVersion

INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(5, 1, 0, GETUTCDATE())

GO

--endUpdatingDatabaseVersion
