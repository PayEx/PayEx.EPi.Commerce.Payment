--beginvalidatingquery
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion')
    BEGIN
    declare @major int = 5, @minor int = 5, @patch int = 0
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch)
        select 0,'Already correct database version'
    ELSE
        select 1, 'Upgrading database'
    END
ELSE
    select -1, 'Not an EPiServer Commerce database'
--endvalidatingquery
GO

CREATE TABLE [dbo].[InventoryService]
(
    [ApplicationId] UNIQUEIDENTIFIER NOT NULL,
    [CatalogEntryCode] NVARCHAR(100) NOT NULL,
    [WarehouseCode] NVARCHAR(50) NOT NULL,
    [IsTracked] BIT NOT NULL,
    [PurchaseAvailableQuantity] DECIMAL(38, 9) NOT NULL,
    [PreorderAvailableQuantity] DECIMAL(38, 9) NOT NULL,
    [BackorderAvailableQuantity] DECIMAL(38, 9) NOT NULL,
    [PurchaseRequestedQuantity] DECIMAL(38, 9) NOT NULL,
    [PreorderRequestedQuantity] DECIMAL(38, 9) NOT NULL,
    [BackorderRequestedQuantity] DECIMAL(38, 9) NOT NULL,
    [PreorderAvailableUtc] DATETIME2 NOT NULL,
    [PurchaseAvailableUtc] DATETIME2 NOT NULL,
    [BackorderAvailableUtc] DATETIME2 NOT NULL,
    [AdditionalQuantity] DECIMAL(38, 9) NOT NULL,
    [ReorderMinQuantity] DECIMAL(38, 9) NOT NULL,
    CONSTRAINT PK_ManagedInventory PRIMARY KEY CLUSTERED ([ApplicationId], [CatalogEntryCode], [WarehouseCode]),
    CONSTRAINT [FK_ManagedInventory_CatalogEntry] FOREIGN KEY ([CatalogEntryCode], [ApplicationId]) REFERENCES [CatalogEntry] ([Code], [ApplicationId]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_ManagedInventory_Warehouse] FOREIGN KEY ([ApplicationId], [WarehouseCode]) REFERENCES [Warehouse] ([ApplicationId], [Code]) ON DELETE CASCADE ON UPDATE CASCADE,
)
GO

CREATE TYPE [dbo].[udttInventory] AS TABLE
(
    [ApplicationId] UNIQUEIDENTIFIER NULL,
    [CatalogEntryCode] NVARCHAR(100) NULL,
    [WarehouseCode] NVARCHAR(50) NULL,
    [IsTracked] BIT NULL,
    [PurchaseAvailableQuantity] DECIMAL(38, 9) NULL,
    [PreorderAvailableQuantity] DECIMAL(38, 9) NULL,
    [BackorderAvailableQuantity] DECIMAL(38, 9) NULL,
    [PurchaseRequestedQuantity] DECIMAL(38, 9) NULL,
    [PreorderRequestedQuantity] DECIMAL(38, 9) NULL,
    [BackorderRequestedQuantity] DECIMAL(38, 9) NULL,
    [PurchaseAvailableUtc] DATETIME2 NULL,
    [PreorderAvailableUtc] DATETIME2 NULL,
    [BackorderAvailableUtc] DATETIME2 NULL,
    [AdditionalQuantity] DECIMAL(38, 9) NULL,
    [ReorderMinQuantity] DECIMAL(38, 9) NULL
)
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_AdjustInventory]
    @changes [dbo].[udttInventory] READONLY
AS
BEGIN
    if exists (
        select 1 from @changes src
        where not exists (select 1 from [dbo].[InventoryService] dst where dst.[ApplicationId] = src.[ApplicationId] and dst.[CatalogEntryCode] = src.[CatalogEntryCode] and dst.[WarehouseCode] = src.[WarehouseCode]))
    begin
        raiserror('unmatched key found in update set', 16, 1)
    end
    else
    begin
        update dst
        set
            [PurchaseAvailableQuantity] = dst.[PurchaseAvailableQuantity] + src.[PurchaseAvailableQuantity],
            [PreorderAvailableQuantity] = dst.[PreorderAvailableQuantity] + src.[PreorderAvailableQuantity],
            [BackorderAvailableQuantity] = dst.[BackorderAvailableQuantity] + src.[BackorderAvailableQuantity],
            [PurchaseRequestedQuantity] = dst.[PurchaseRequestedQuantity] + src.[PurchaseRequestedQuantity],
            [PreorderRequestedQuantity] = dst.[PreorderRequestedQuantity] + src.[PreorderRequestedQuantity],
            [BackorderRequestedQuantity] = dst.[BackorderRequestedQuantity] + src.[BackorderRequestedQuantity]
        from [dbo].[InventoryService] dst
        join (
            select
                [ApplicationId], [CatalogEntryCode], [WarehouseCode],
                SUM([PurchaseAvailableQuantity]) as [PurchaseAvailableQuantity],
                SUM([PreorderAvailableQuantity]) as [PreorderAvailableQuantity],
                SUM([BackorderAvailableQuantity]) as [BackorderAvailableQuantity],
                SUM([PurchaseRequestedQuantity]) as [PurchaseRequestedQuantity],
                SUM([PreorderRequestedQuantity]) as [PreorderRequestedQuantity],
                SUM([BackorderRequestedQuantity]) as [BackorderRequestedQuantity]
            from @changes
            group by [ApplicationId], [CatalogEntryCode], [WarehouseCode]) src
          on dst.[ApplicationId] = src.[ApplicationId] and dst.[CatalogEntryCode] = src.[CatalogEntryCode] and dst.[WarehouseCode] = src.[WarehouseCode]
    end
END
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_DeleteInventory]
    @partialKeys [dbo].[udttInventory] READONLY
AS
BEGIN
    delete mi
    from [dbo].[InventoryService] mi
    where exists (
        select 1
        from @partialKeys keys
        where mi.[ApplicationId] = keys.[ApplicationId]
          and mi.[CatalogEntryCode] = isnull(keys.[CatalogEntryCode], mi.[CatalogEntryCode])
          and mi.[WarehouseCode] = isnull(keys.[WarehouseCode], mi.[WarehouseCode]))
END
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_DeleteWarehouse]
    @ApplicationId UNIQUEIDENTIFIER,
    @Code NVARCHAR(50)
AS
BEGIN
    delete from [dbo].[Warehouse]
    where [ApplicationId] = @ApplicationId and [Code] = @Code
END
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_GetInventory]
    @ApplicationId uniqueidentifier,
    @CatalogEntryCode nvarchar(100),
    @WarehouseCode nvarchar(50)
AS
BEGIN
    select
        [ApplicationId],
        [CatalogEntryCode],
        [WarehouseCode],
        [IsTracked],
        [PurchaseAvailableQuantity],
        [PreorderAvailableQuantity],
        [BackorderAvailableQuantity],
        [PurchaseRequestedQuantity],
        [PreorderRequestedQuantity],
        [BackorderRequestedQuantity],
        [PurchaseAvailableUtc],
        [PreorderAvailableUtc],
        [BackorderAvailableUtc],
        [AdditionalQuantity],
        [ReorderMinQuantity]
    from [dbo].[InventoryService]
    where [ApplicationId] = @ApplicationId and [CatalogEntryCode] = @CatalogEntryCode and [WarehouseCode] = @WarehouseCode
END
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_GetWarehouse]
    @ApplicationId uniqueidentifier,
    @Code nvarchar(50)
AS
BEGIN
    select
        [WarehouseId],
        [ApplicationId],
        [Name],
        [CreatorId],
        [Created],
        [ModifierId],
        [Modified],
        [IsActive],
        [IsPrimary],
        [SortOrder],
        [Code],
        [IsFulfillmentCenter],
        [IsPickupLocation],
        [IsDeliveryLocation],
        [FirstName],
        [LastName],
        [Organization],
        [Line1],
        [Line2],
        [City],
        [State],
        [CountryCode],
        [CountryName],
        [PostalCode],
        [RegionCode],
        [RegionName],
        [DaytimePhoneNumber],
        [EveningPhoneNumber],
        [FaxNumber],
        [Email]
    from [dbo].[Warehouse]
    where [ApplicationId] = @ApplicationId and [Code] = @Code
END
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_InsertInventory]
    @inventory [dbo].[udttInventory] READONLY
AS
BEGIN
    insert into [dbo].[InventoryService]
    (
        [ApplicationId],
        [CatalogEntryCode],
        [WarehouseCode],
        [IsTracked],
        [PurchaseAvailableQuantity],
        [PreorderAvailableQuantity],
        [BackorderAvailableQuantity],
        [PurchaseRequestedQuantity],
        [PreorderRequestedQuantity],
        [BackorderRequestedQuantity],
        [PurchaseAvailableUtc],
        [PreorderAvailableUtc],
        [BackorderAvailableUtc],
        [AdditionalQuantity],
        [ReorderMinQuantity]
    )
    select
        src.[ApplicationId],
        src.[CatalogEntryCode],
        src.[WarehouseCode],
        src.[IsTracked],
        src.[PurchaseAvailableQuantity],
        src.[PreorderAvailableQuantity],
        src.[BackorderAvailableQuantity],
        src.[PurchaseRequestedQuantity],
        src.[PreorderRequestedQuantity],
        src.[BackorderRequestedQuantity],
        src.[PurchaseAvailableUtc],
        src.[PreorderAvailableUtc],
        src.[BackorderAvailableUtc],
        src.[AdditionalQuantity],
        src.[ReorderMinQuantity]
    from @inventory src
END
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_InsertWarehouse]
    @ApplicationId UNIQUEIDENTIFIER,
    @Name NVARCHAR(255),
    @CreatorId NVARCHAR(100),
    @Created DATETIME,
    @ModifierId NVARCHAR(100),
    @Modified DATETIME,
    @IsActive BIT,
    @IsPrimary BIT,
    @SortOrder INT,
    @Code NVARCHAR(50),
    @IsFulfillmentCenter BIT,
    @IsPickupLocation BIT,
    @IsDeliveryLocation BIT,
    @FirstName NVARCHAR(64),
    @LastName NVARCHAR(64),
    @Organization NVARCHAR(64),
    @Line1 NVARCHAR(80),
    @Line2 NVARCHAR(80),
    @City NVARCHAR(64),
    @State NVARCHAR(64),
    @CountryCode NVARCHAR(50),
    @CountryName NVARCHAR(50),
    @PostalCode NVARCHAR(20),
    @RegionCode NVARCHAR(50),
    @RegionName NVARCHAR(64),
    @DaytimePhoneNumber NVARCHAR(32),
    @EveningPhoneNumber NVARCHAR(32),
    @FaxNumber NVARCHAR(32),
    @Email NVARCHAR(64)
AS
BEGIN
    insert into [dbo].[Warehouse] (
        [ApplicationId],
        [Name],
        [CreatorId],
        [Created],
        [ModifierId],
        [Modified],
        [IsActive],
        [IsPrimary],
        [SortOrder],
        [Code],
        [IsFulfillmentCenter],
        [IsPickupLocation],
        [IsDeliveryLocation],
        [FirstName],
        [LastName],
        [Organization],
        [Line1],
        [Line2],
        [City],
        [State],
        [CountryCode],
        [CountryName],
        [PostalCode],
        [RegionCode],
        [RegionName],
        [DaytimePhoneNumber],
        [EveningPhoneNumber],
        [FaxNumber],
        [Email]
    ) values (
        @ApplicationId,
        @Name,
        @CreatorId,
        @Created,
        @ModifierId,
        @Modified,
        @IsActive,
        @IsPrimary,
        @SortOrder,
        @Code,
        @IsFulfillmentCenter,
        @IsPickupLocation,
        @IsDeliveryLocation,
        @FirstName,
        @LastName,
        @Organization,
        @Line1,
        @Line2,
        @City,
        @State,
        @CountryCode,
        @CountryName,
        @PostalCode,
        @RegionCode,
        @RegionName,
        @DaytimePhoneNumber,
        @EveningPhoneNumber,
        @FaxNumber,
        @Email
    )
END
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_ListInventory]
    @ApplicationId uniqueidentifier
AS
BEGIN
    select
        [ApplicationId],
        [CatalogEntryCode],
        [WarehouseCode],
        [IsTracked],
        [PurchaseAvailableQuantity],
        [PreorderAvailableQuantity],
        [BackorderAvailableQuantity],
        [PurchaseRequestedQuantity],
        [PreorderRequestedQuantity],
        [BackorderRequestedQuantity],
        [PurchaseAvailableUtc],
        [PreorderAvailableUtc],
        [BackorderAvailableUtc],
        [AdditionalQuantity],
        [ReorderMinQuantity]
    from [dbo].[InventoryService]
    where [ApplicationId] = @ApplicationId
    order by [CatalogEntryCode], [WarehouseCode]
END
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_ListWarehouses]
    @ApplicationId uniqueidentifier
AS
BEGIN
    select
        [WarehouseId],
        [ApplicationId],
        [Name],
        [CreatorId],
        [Created],
        [ModifierId],
        [Modified],
        [IsActive],
        [IsPrimary],
        [SortOrder],
        [Code],
        [IsFulfillmentCenter],
        [IsPickupLocation],
        [IsDeliveryLocation],
        [FirstName],
        [LastName],
        [Organization],
        [Line1],
        [Line2],
        [City],
        [State],
        [CountryCode],
        [CountryName],
        [PostalCode],
        [RegionCode],
        [RegionName],
        [DaytimePhoneNumber],
        [EveningPhoneNumber],
        [FaxNumber],
        [Email]
    from [dbo].[Warehouse]
    where [ApplicationId] = @applicationId
END
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_QueryInventory]
    @partialKeys [dbo].[udttInventory] READONLY
AS
BEGIN
    select
        [ApplicationId],
        [CatalogEntryCode],
        [WarehouseCode],
        [IsTracked],
        [PurchaseAvailableQuantity],
        [PreorderAvailableQuantity],
        [BackorderAvailableQuantity],
        [PurchaseRequestedQuantity],
        [PreorderRequestedQuantity],
        [BackorderRequestedQuantity],
        [PurchaseAvailableUtc],
        [PreorderAvailableUtc],
        [BackorderAvailableUtc],
        [AdditionalQuantity],
        [ReorderMinQuantity]
    from [dbo].[InventoryService] mi
    where exists (
        select 1
        from @partialKeys keys
        where mi.[ApplicationId] = keys.[ApplicationId]
          and mi.[CatalogEntryCode] = isnull(keys.[CatalogEntryCode], mi.[CatalogEntryCode])
          and mi.[WarehouseCode] = isnull(keys.[WarehouseCode], mi.[WarehouseCode]))
    order by [ApplicationId], [CatalogEntryCode], [WarehouseCode]
END
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_QueryInventoryPaged]
    @offset int,
    @count int,
    @partialKeys [dbo].[udttInventory] READONLY
AS
BEGIN
    declare @results table (
        [ApplicationId] uniqueidentifier,
        [CatalogEntryCode] nvarchar(100),
        [WarehouseCode] nvarchar(50),
        [IsTracked] bit,
        [PurchaseAvailableQuantity] decimal(38, 9),
        [PreorderAvailableQuantity] decimal(38, 9),
        [BackorderAvailableQuantity] decimal(38, 9),
        [PurchaseRequestedQuantity] decimal(38, 9),
        [PreorderRequestedQuantity] decimal(38, 9),
        [BackorderRequestedQuantity] decimal(38, 9),
        [PurchaseAvailableUtc] datetime2,
        [PreorderAvailableUtc] datetime2,
        [BackorderAvailableUtc] datetime2,
        [AdditionalQuantity] decimal(38, 9),
        [ReorderMinQuantity] decimal(38, 9),
        [RowNumber] int,
        [TotalCount] int
    )

    insert into @results (
        [ApplicationId],
        [CatalogEntryCode],
        [WarehouseCode],
        [IsTracked],
        [PurchaseAvailableQuantity],
        [PreorderAvailableQuantity],
        [BackorderAvailableQuantity],
        [PurchaseRequestedQuantity],
        [PreorderRequestedQuantity],
        [BackorderRequestedQuantity],
        [PurchaseAvailableUtc],
        [PreorderAvailableUtc],
        [BackorderAvailableUtc],
        [AdditionalQuantity],
        [ReorderMinQuantity],
        [RowNumber],
        [TotalCount]
    )
    select
        [ApplicationId],
        [CatalogEntryCode],
        [WarehouseCode],
        [IsTracked],
        [PurchaseAvailableQuantity],
        [PreorderAvailableQuantity],
        [BackorderAvailableQuantity],
        [PurchaseRequestedQuantity],
        [PreorderRequestedQuantity],
        [BackorderRequestedQuantity],
        [PurchaseAvailableUtc],
        [PreorderAvailableUtc],
        [BackorderAvailableUtc],
        [AdditionalQuantity],
        [ReorderMinQuantity],
        [RowNumber],
        [RowNumber] + [ReverseRowNumber] - 1 as [TotalCount]
    from (
        select
            ROW_NUMBER() over (order by [ApplicationId], [CatalogEntryCode], [WarehouseCode]) as [RowNumber],
            ROW_NUMBER() over (order by [ApplicationId] desc, [CatalogEntryCode] desc, [WarehouseCode] desc) as [ReverseRowNumber],
            [ApplicationId],
            [CatalogEntryCode],
            [WarehouseCode],
            [IsTracked],
            [PurchaseAvailableQuantity],
            [PreorderAvailableQuantity],
            [BackorderAvailableQuantity],
            [PurchaseRequestedQuantity],
            [PreorderRequestedQuantity],
            [BackorderRequestedQuantity],
            [PurchaseAvailableUtc],
            [PreorderAvailableUtc],
            [BackorderAvailableUtc],
            [AdditionalQuantity],
            [ReorderMinQuantity]
        from [dbo].[InventoryService] mi
        where exists (
            select 1
            from @partialKeys keys
            where mi.[ApplicationId] = keys.[ApplicationId]
              and mi.[CatalogEntryCode] = isnull(keys.[CatalogEntryCode], mi.[CatalogEntryCode])
              and mi.[WarehouseCode] = isnull(keys.[WarehouseCode], mi.[WarehouseCode]))
    ) paged
    where @offset < [RowNumber] and [RowNumber] <= (@offset + @count)

    if not exists (select 1 from @results)
    begin
        select COUNT(*) as TotalCount
        from [dbo].[InventoryService] mi
        where exists (
            select 1
            from @partialKeys keys
            where mi.[ApplicationId] = keys.[ApplicationId]
              and mi.[CatalogEntryCode] = isnull(keys.[CatalogEntryCode], mi.[CatalogEntryCode])
              and mi.[WarehouseCode] = isnull(keys.[WarehouseCode], mi.[WarehouseCode]))
    end
    else
    begin
        select top 1 [TotalCount] from @results
    end

    select
        [ApplicationId],
        [CatalogEntryCode],
        [WarehouseCode],
        [IsTracked],
        [PurchaseAvailableQuantity],
        [PreorderAvailableQuantity],
        [BackorderAvailableQuantity],
        [PurchaseRequestedQuantity],
        [PreorderRequestedQuantity],
        [BackorderRequestedQuantity],
        [PurchaseAvailableUtc],
        [PreorderAvailableUtc],
        [BackorderAvailableUtc],
        [AdditionalQuantity],
        [ReorderMinQuantity]
    from @results
    order by [RowNumber]
END
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_SaveInventory]
    @inventory [dbo].[udttInventory] READONLY
AS
BEGIN
    merge into [dbo].[InventoryService] dst
    using @inventory src
    on (dst.[ApplicationId] = src.[ApplicationId] and dst.[CatalogEntryCode] = src.[CatalogEntryCode] and dst.[WarehouseCode] = src.[WarehouseCode])
    when matched then
        update set
            [IsTracked] = src.[IsTracked],
            [PurchaseAvailableQuantity] = src.[PurchaseAvailableQuantity],
            [PreorderAvailableQuantity] = src.[PreorderAvailableQuantity],
            [BackorderAvailableQuantity] = src.[BackorderAvailableQuantity],
            [PurchaseRequestedQuantity] = src.[PurchaseRequestedQuantity],
            [PreorderRequestedQuantity] = src.[PreorderRequestedQuantity],
            [BackorderRequestedQuantity] = src.[BackorderRequestedQuantity],
            [PurchaseAvailableUtc] = src.[PurchaseAvailableUtc],
            [PreorderAvailableUtc] = src.[PreorderAvailableUtc],
            [BackorderAvailableUtc] = src.[BackorderAvailableUtc],
            [AdditionalQuantity] = src.[AdditionalQuantity],
            [ReorderMinQuantity] = src.[ReorderMinQuantity]
    when not matched then
        insert (
            [ApplicationId],
            [CatalogEntryCode],
            [WarehouseCode],
            [IsTracked],
            [PurchaseAvailableQuantity],
            [PreorderAvailableQuantity],
            [BackorderAvailableQuantity],
            [PurchaseRequestedQuantity],
            [PreorderRequestedQuantity],
            [BackorderRequestedQuantity],
            [PurchaseAvailableUtc],
            [PreorderAvailableUtc],
            [BackorderAvailableUtc],
            [AdditionalQuantity],
            [ReorderMinQuantity]
        ) values (
            src.[ApplicationId],
            src.[CatalogEntryCode],
            src.[WarehouseCode],
            src.[IsTracked],
            src.[PurchaseAvailableQuantity],
            src.[PreorderAvailableQuantity],
            src.[BackorderAvailableQuantity],
            src.[PurchaseRequestedQuantity],
            src.[PreorderRequestedQuantity],
            src.[BackorderRequestedQuantity],
            src.[PurchaseAvailableUtc],
            src.[PreorderAvailableUtc],
            src.[BackorderAvailableUtc],
            src.[AdditionalQuantity],
            src.[ReorderMinQuantity]
        );
END
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_SaveWarehouse]
    @ApplicationId UNIQUEIDENTIFIER,
    @Name NVARCHAR(255),
    @CreatorId NVARCHAR(100),
    @Created DATETIME,
    @ModifierId NVARCHAR(100),
    @Modified DATETIME,
    @IsActive BIT,
    @IsPrimary BIT,
    @SortOrder INT,
    @Code NVARCHAR(50),
    @IsFulfillmentCenter BIT,
    @IsPickupLocation BIT,
    @IsDeliveryLocation BIT,
    @FirstName NVARCHAR(64),
    @LastName NVARCHAR(64),
    @Organization NVARCHAR(64),
    @Line1 NVARCHAR(80),
    @Line2 NVARCHAR(80),
    @City NVARCHAR(64),
    @State NVARCHAR(64),
    @CountryCode NVARCHAR(50),
    @CountryName NVARCHAR(50),
    @PostalCode NVARCHAR(20),
    @RegionCode NVARCHAR(50),
    @RegionName NVARCHAR(64),
    @DaytimePhoneNumber NVARCHAR(32),
    @EveningPhoneNumber NVARCHAR(32),
    @FaxNumber NVARCHAR(32),
    @Email NVARCHAR(64)
AS
BEGIN
    if exists (select 1 from [dbo].[Warehouse] where [ApplicationId] = @ApplicationId and [Code] = @Code)
    begin
        exec [dbo].[ecf_Inventory_UpdateWarehouse]
            @ApplicationId,
            @Name,
            @CreatorId,
            @Created,
            @ModifierId,
            @Modified,
            @IsActive,
            @IsPrimary,
            @SortOrder,
            @Code,
            @IsFulfillmentCenter,
            @IsPickupLocation,
            @IsDeliveryLocation,
            @FirstName,
            @LastName,
            @Organization,
            @Line1,
            @Line2,
            @City,
            @State,
            @CountryCode,
            @CountryName,
            @PostalCode,
            @RegionCode,
            @RegionName,
            @DaytimePhoneNumber,
            @EveningPhoneNumber,
            @FaxNumber,
            @Email
    end
    else
    begin
        exec [dbo].[ecf_Inventory_InsertWarehouse]
            @ApplicationId,
            @Name,
            @CreatorId,
            @Created,
            @ModifierId,
            @Modified,
            @IsActive,
            @IsPrimary,
            @SortOrder,
            @Code,
            @IsFulfillmentCenter,
            @IsPickupLocation,
            @IsDeliveryLocation,
            @FirstName,
            @LastName,
            @Organization,
            @Line1,
            @Line2,
            @City,
            @State,
            @CountryCode,
            @CountryName,
            @PostalCode,
            @RegionCode,
            @RegionName,
            @DaytimePhoneNumber,
            @EveningPhoneNumber,
            @FaxNumber,
            @Email
    end
END
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_UpdateInventory]
    @inventory [dbo].[udttInventory] READONLY
AS
BEGIN
    if exists (
        select 1 from @inventory
        group by [ApplicationId], [CatalogEntryCode], [WarehouseCode]
        having COUNT(*) > 1)
    begin
        raiserror('duplicate key found in update set', 16, 1)
    end
    else if exists (
        select 1 from @inventory src
        where not exists (select 1 from [dbo].[InventoryService] dst where dst.[ApplicationId] = src.[ApplicationId] and dst.[CatalogEntryCode] = src.[CatalogEntryCode] and dst.[WarehouseCode] = src.[WarehouseCode]))
    begin
        raiserror('unmatched key found in update set', 16, 1)
    end
    else
    begin
        update dst
        set
            [IsTracked] = src.[IsTracked],
            [PurchaseAvailableQuantity] = src.[PurchaseAvailableQuantity],
            [PreorderAvailableQuantity] = src.[PreorderAvailableQuantity],
            [BackorderAvailableQuantity] = src.[BackorderAvailableQuantity],
            [PurchaseRequestedQuantity] = src.[PurchaseRequestedQuantity],
            [PreorderRequestedQuantity] = src.[PreorderRequestedQuantity],
            [BackorderRequestedQuantity] = src.[BackorderRequestedQuantity],
            [PurchaseAvailableUtc] = src.[PurchaseAvailableUtc],
            [PreorderAvailableUtc] = src.[PreorderAvailableUtc],
            [BackorderAvailableUtc] = src.[BackorderAvailableUtc],
            [AdditionalQuantity] = src.[AdditionalQuantity],
            [ReorderMinQuantity] = src.[ReorderMinQuantity]
        from [dbo].[InventoryService] dst
        join @inventory src on dst.[ApplicationId] = src.[ApplicationId] and dst.[CatalogEntryCode] = src.[CatalogEntryCode] and dst.[WarehouseCode] = src.[WarehouseCode]
    end
END
GO

CREATE PROCEDURE [dbo].[ecf_Inventory_UpdateWarehouse]
    @ApplicationId UNIQUEIDENTIFIER,
    @Name NVARCHAR(255),
    @CreatorId NVARCHAR(100),
    @Created DATETIME,
    @ModifierId NVARCHAR(100),
    @Modified DATETIME,
    @IsActive BIT,
    @IsPrimary BIT,
    @SortOrder INT,
    @Code NVARCHAR(50),
    @IsFulfillmentCenter BIT,
    @IsPickupLocation BIT,
    @IsDeliveryLocation BIT,
    @FirstName NVARCHAR(64),
    @LastName NVARCHAR(64),
    @Organization NVARCHAR(64),
    @Line1 NVARCHAR(80),
    @Line2 NVARCHAR(80),
    @City NVARCHAR(64),
    @State NVARCHAR(64),
    @CountryCode NVARCHAR(50),
    @CountryName NVARCHAR(50),
    @PostalCode NVARCHAR(20),
    @RegionCode NVARCHAR(50),
    @RegionName NVARCHAR(64),
    @DaytimePhoneNumber NVARCHAR(32),
    @EveningPhoneNumber NVARCHAR(32),
    @FaxNumber NVARCHAR(32),
    @Email NVARCHAR(64)
AS
BEGIN
    update dbo.Warehouse
    set
        [ApplicationId] = @ApplicationId,
        [Name] = @Name,
        [CreatorId] = @CreatorId,
        [Created] = @Created,
        [ModifierId] = @ModifierId,
        [Modified] = @Modified,
        [IsActive] = @IsActive,
        [IsPrimary] = @IsPrimary,
        [SortOrder] = @SortOrder,
        [Code] = @Code,
        [IsFulfillmentCenter] = @IsFulfillmentCenter,
        [IsPickupLocation] = @IsPickupLocation,
        [IsDeliveryLocation] = @IsDeliveryLocation,
        [FirstName] = @FirstName,
        [LastName] = @LastName,
        [Organization] = @Organization,
        [Line1] = @Line1,
        [Line2] = @Line2,
        [City] = @City,
        [State] = @State,
        [CountryCode] = @CountryCode,
        [CountryName] = @CountryName,
        [PostalCode] = @PostalCode,
        [RegionCode] = @RegionCode,
        [RegionName] = @RegionName,
        [DaytimePhoneNumber] = @DaytimePhoneNumber,
        [EveningPhoneNumber] = @EveningPhoneNumber,
        [FaxNumber] = @FaxNumber,
        [Email] = @Email
    where [ApplicationId] = @ApplicationId and [Code] = @Code
END
GO

--beginUpdatingDatabaseVersion

INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(5, 5, 0, GETUTCDATE())

GO

--endUpdatingDatabaseVersion
