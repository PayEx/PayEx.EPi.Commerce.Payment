--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 6, @minor int = 3, @patch int = 2  
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
 
GO 

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_CatalogEntrySearch_Init]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_CatalogEntrySearch_Init] 
GO 

CREATE procedure [dbo].[ecf_CatalogEntrySearch_Init]
    @ApplicationId uniqueidentifier,
    @CatalogId int,
    @SearchSetId uniqueidentifier,
    @IncludeInactive bit,
    @EarliestModifiedDate datetime = null,
    @LatestModifiedDate datetime = null,
    @DatabaseClockOffsetMS int = null
as
begin
	declare @purgedate datetime
	begin try
		set @purgedate = datediff(day, 3, GETUTCDATE())
		delete from [CatalogEntrySearchResults_SingleSort] where Created < @purgedate
	end try
	begin catch
	end catch

    declare @MetaTableName sysname
    declare @CatalogEntryIdSubquery nvarchar(max)
    declare @ModifiedFilter nvarchar(4000)
    declare @query nvarchar(max)
    set @CatalogEntryIdSubquery = null
    
    -- @ModifiedFilter: if there is a filter, build the where clause for it here.
    if (@EarliestModifiedDate is not null and @LatestModifiedDate is not null) set @ModifiedFilter = ' where Modified between cast(''' + CONVERT(nvarchar(100), @EarliestModifiedDate, 127) + ''' as datetime) and cast('''  + CONVERT(nvarchar(100), @LatestModifiedDate, 127) + ''' as datetime)'
    else if (@EarliestModifiedDate is not null) set @ModifiedFilter = ' where Modified >= cast(''' + CONVERT(nvarchar(100), @EarliestModifiedDate, 127) + ''' as datetime)'
    else if (@LatestModifiedDate is not null) set @ModifiedFilter = ' where Modified <= cast('''  + CONVERT(nvarchar(100), @LatestModifiedDate, 127) + ''' as datetime)'
    else set @ModifiedFilter = ''
    
    -- @MetaTableSubquery: find all the metaclass tables, and fetch a union of all their keys, applying the @ModifiedFilter.
    declare metatables_cursor cursor local read_only for
        select childClass.TableName
        from MetaClass parentClass
        join MetaClass childClass on parentClass.MetaClassId = childClass.ParentClassId
        where childClass.IsSystem = 0
          and parentClass.Name = 'CatalogEntry'
    open metatables_cursor
    fetch metatables_cursor into @MetaTableName
    while (@@FETCH_STATUS = 0)
    begin
        set @CatalogEntryIdSubquery = 
            case when @CatalogEntryIdSubquery is null then '' else @CatalogEntryIdSubquery + ' union all ' end +
            'select ObjectId from ' + @MetaTableName + @ModifiedFilter
            
        fetch metatables_cursor into @MetaTableName        
    end
    close metatables_cursor
    deallocate metatables_cursor

    -- more @CatalogEntryIdSubquery: find all the catalog entries that have modified relations in NodeEntryRelation, or deleted relations in ApplicationLog
    if (@EarliestModifiedDate is not null and @LatestModifiedDate is not null)
    begin
        -- adjust modified date filters to account for clock difference between database server and application server clocks    
        if (@EarliestModifiedDate is not null and isnull(@DatabaseClockOffsetMS, 0) > 0)
        begin
            set @EarliestModifiedDate = DATEADD(MS, -@DatabaseClockOffsetMS, @EarliestModifiedDate)
        
            if (@EarliestModifiedDate is not null and @LatestModifiedDate is not null) set @ModifiedFilter = ' where Modified between cast(''' + CONVERT(nvarchar(100), @EarliestModifiedDate, 127) + ''' as datetime) and cast('''  + CONVERT(nvarchar(100), @LatestModifiedDate, 127) + ''' as datetime)'
            else if (@EarliestModifiedDate is not null) set @ModifiedFilter = ' where Modified >= cast(''' + CONVERT(nvarchar(100), @EarliestModifiedDate, 127) + ''' as datetime)'
            else if (@LatestModifiedDate is not null) set @ModifiedFilter = ' where Modified <= cast('''  + CONVERT(nvarchar(100), @LatestModifiedDate, 127) + ''' as datetime)'
            else set @ModifiedFilter = ''    
        end
    
        declare @ApplicationLogCreatedFilter nvarchar(4000)
        set @ApplicationLogCreatedFilter = REPLACE(REPLACE(@ModifiedFilter, ' where ', ' and '), 'Modified', 'Created')
        
        set @CatalogEntryIdSubquery =
            case when @CatalogEntryIdSubquery is null then '' else @CatalogEntryIdSubquery + ' union all ' end +
            'select CatalogEntryId from NodeEntryRelation' + @ModifiedFilter +
            ' union all ' +
            'select cast(ObjectKey as int) as CatalogEntryId from ApplicationLog where [Source] = ''catalog'' and [Operation] = ''Modified'' and [ObjectType] = ''relation''' + @ApplicationLogCreatedFilter
    end
   
    set @query = 
    'insert into CatalogEntrySearchResults_SingleSort (SearchSetId, ResultIndex, CatalogEntryId, ApplicationId) ' +
    'select ''' + cast(@SearchSetId as nvarchar(36)) + ''', ROW_NUMBER() over (order by CatalogEntryId), CatalogEntryId, ApplicationId ' +
    'from CatalogEntry ' +
    'where CatalogEntry.ApplicationId = ''' + cast(@ApplicationId as nvarchar(36)) + ''' ' +
      'and CatalogEntry.CatalogId = ' + cast(@CatalogId as nvarchar) + ' ' +
      'and CatalogEntry.CatalogEntryId in (' + @CatalogEntryIdSubquery + ')'
      
    if @IncludeInactive = 0 set @query = @query + ' and CatalogEntry.IsActive = 1'

    execute dbo.sp_executesql @query
    
    select @@ROWCOUNT
end

GO
 
--beginUpdatingDatabaseVersion 
 
INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(6, 3, 2, GETUTCDATE()) 
 
GO 

--endUpdatingDatabaseVersion 
