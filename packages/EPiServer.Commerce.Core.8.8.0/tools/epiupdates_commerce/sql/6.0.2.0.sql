--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 6, @minor int = 0, @patch int = 2    
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_reporting_LowStock]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_reporting_LowStock]
GO

CREATE PROCEDURE [dbo].[ecf_reporting_LowStock] 
    @ApplicationID uniqueidentifier
As


BEGIN

    SELECT E.[Name], E.Code as SkuId, I.BackorderAvailableUtc as [BackorderAvailabilityDate],
    I.PreorderAvailableUtc as [PreorderAvailabilityDate],
    I.IsTracked as [InventoryStatus],
    [AllowBackorder] = 
        CASE 
            WHEN I.BackorderAvailableQuantity > 0 THEN 1
            ELSE 0
        END,
    [AllowPreOrder] = 
        CASE 
            WHEN I.PreorderAvailableUtc > convert(datetime,0x0000000000000000) THEN 1
            ELSE 0
        END,
    I.BackorderAvailableQuantity as [BackorderQuantity],
    I.PreorderAvailableQuantity as [PreorderQuantity],
    I.ReorderMinQuantity,
    I.WarehouseCode,
    I.AdditionalQuantity as [ReservedQuantity],
    I.PurchaseAvailableQuantity + I.AdditionalQuantity as [InstockQuantity],
    W.Name as WarehouseName from [InventoryService] I
    INNER JOIN [CatalogEntry] E ON E.Code = I.CatalogEntryCode 
    INNER JOIN Catalog C ON C.CatalogId = E.CatalogId
    INNER JOIN [Warehouse] W ON I.WarehouseCode = W.Code
    WHERE I.PurchaseAvailableQuantity < I.ReorderMinQuantity AND I.IsTracked <> 0 
    AND C.ApplicationId = @ApplicationID

END
GO


IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_CatalogEntrySearch_GetResults]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_CatalogEntrySearch_GetResults]
GO

create procedure [dbo].[ecf_CatalogEntrySearch_GetResults]
    @SearchSetId uniqueidentifier,
    @FirstResultIndex int,
    @MaxResultCount int
as
begin
    declare @LastResultIndex int
    declare @ApplicationId uniqueidentifier
    set @LastResultIndex = @FirstResultIndex + @MaxResultCount - 1
    
    declare @keyset table (CatalogEntryId int, ApplicationId uniqueidentifier)
    insert into @keyset 
    select CatalogEntryId, ApplicationId
    from CatalogEntrySearchResults_SingleSort ix
    where ix.SearchSetId = @SearchSetId
      and ix.ResultIndex between @FirstResultIndex and @LastResultIndex
    
    select top 1 @ApplicationId = ApplicationId
     from @keyset
      
    select ce.*
    from CatalogEntry ce
    join @keyset ks on ce.CatalogEntryId = ks.CatalogEntryId
    order by ce.CatalogEntryId
    
    select cis.*
    from CatalogItemSeo cis
    join @keyset ks on cis.CatalogEntryId = ks.CatalogEntryId
    where cis.ApplicationId=@ApplicationId
    order by cis.CatalogEntryId
    
    select v.*
    from Variation v
    join @keyset ks on v.CatalogEntryId = ks.CatalogEntryId
    order by v.CatalogEntryId
                    
    select distinct m.*
    from Merchant m
    join Variation v on m.MerchantId = v.MerchantId
    join @keyset ks on v.CatalogEntryId = ks.CatalogEntryId
    where m.ApplicationId=@ApplicationId
        
    select ca.*
    from CatalogAssociation ca
    join @keyset ks on ca.CatalogEntryId = ks.CatalogEntryId
    order by ca.CatalogEntryId

    select cia.*
    from CatalogItemAsset cia
    join @keyset ks on cia.CatalogEntryId = ks.CatalogEntryId
    order by cia.CatalogEntryId

    select ner.*
    from NodeEntryRelation ner
    join @keyset ks on ner.CatalogEntryId = ks.CatalogEntryId
    order by ner.CatalogEntryId

    -- Cleanup the loaded OrderGroupIds from SearchResults.
    delete from CatalogEntrySearchResults_SingleSort
    where @SearchSetId = SearchSetId and ResultIndex between @FirstResultIndex and @LastResultIndex
end
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_CatalogEntry_List]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_CatalogEntry_List]
GO

create procedure dbo.ecf_CatalogEntry_List
    @CatalogEntries dbo.udttEntityList readonly
as
begin
    select n.*
    from CatalogEntry n
    join @CatalogEntries r on n.CatalogEntryId = r.EntityId
    order by r.SortOrder
    
    select s.*
    from CatalogItemSeo s
    join @CatalogEntries r on s.CatalogEntryId = r.EntityId

    select v.*
    from Variation v
    join @CatalogEntries r on v.CatalogEntryId = r.EntityId

    select m.*
    from Merchant m
    join Variation v on m.MerchantId = v.MerchantId
    join @CatalogEntries r on v.CatalogEntryId = r.EntityId
    
    select a.*
    from CatalogAssociation a
    join @CatalogEntries r on a.CatalogEntryId = r.EntityId

    select a.*
    from CatalogItemAsset a
    join @CatalogEntries r on a.CatalogEntryId = r.EntityId

    select er.CatalogId, er.CatalogEntryId, er.CatalogNodeId, er.SortOrder
    from NodeEntryRelation er
    join @CatalogEntries r on er.CatalogEntryId = r.EntityId
end

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_CatalogEntry_Inventory]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_CatalogEntry_Inventory]
GO

--beginUpdatingDatabaseVersion 
 
INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(6, 0, 2, GETUTCDATE()) 
GO 

--endUpdatingDatabaseVersion 