--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 5, @minor int = 6, @patch int = 1   
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
 
GO

ALTER PROCEDURE [dbo].[mdpsp_sys_CreateMetaClass]
	@Namespace 		NVARCHAR(1024),
	@Name 		NVARCHAR(256),
	@FriendlyName		NVARCHAR(256),
	@TableName 		NVARCHAR(256),
	@ParentClassId 		INT,
	@IsSystem		BIT,
	@IsAbstract		BIT	=	0,
	@Description 		NTEXT,
	@Retval 		INT OUTPUT
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
			EXEC('CREATE TABLE [dbo].[' + @TableName  + '] ([ObjectId] [int] NOT NULL , [CreatorId] [nvarchar](100), [Created] [datetime], [ModifierId] [nvarchar](100) , [Modified] [datetime] ) ON [PRIMARY]')

			IF @@ERROR <> 0 GOTO ERR

			EXEC('ALTER TABLE [dbo].[' + @TableName  + '] WITH NOCHECK ADD CONSTRAINT [PK_' + @TableName  + '] PRIMARY KEY  CLUSTERED ([ObjectId])  ON [PRIMARY]')

			IF @@ERROR <> 0 GOTO ERR

			IF EXISTS(SELECT * FROM MetaClass WHERE MetaClassId = @ParentClassId /* AND @IsSystem = 1 */ )
			BEGIN
				-- Step 3-2. Insert a new record in to the MetaClassMetaFieldRelation table
				INSERT INTO [MetaClassMetaFieldRelation]  (MetaClassId, MetaFieldId)
					SELECT @Retval, MetaFieldId FROM MetaField WHERE [SystemMetaClassId] = @ParentClassId
			END

			IF @@ERROR<> 0 GOTO ERR

			-- Step 2-2. Create the @TableName_Localization table
			EXEC('CREATE TABLE [dbo].[' + @TableName + '_Localization] ([Id] [int] IDENTITY (1, 1)  NOT NULL, [ObjectId] [int] NOT NULL , [ModifierId] [nvarchar](100), [Modified] [datetime], [Language] nvarchar(20) NOT NULL) ON [PRIMARY]')

			IF @@ERROR<> 0 GOTO ERR

			EXEC('ALTER TABLE [dbo].[' + @TableName  + '_Localization] WITH NOCHECK ADD CONSTRAINT [PK_' + @TableName  + '_Localization] PRIMARY KEY  CLUSTERED ([Id])  ON [PRIMARY]')

			IF @@ERROR<> 0 GOTO ERR

			EXEC ('CREATE NONCLUSTERED INDEX IX_' + @TableName + '_Localization_Language ON dbo.' + @TableName + '_Localization ([Language]) ON [PRIMARY]')

			IF @@ERROR<> 0 GOTO ERR

			EXEC ('CREATE UNIQUE NONCLUSTERED INDEX IX_' + @TableName + '_Localization_ObjectId ON dbo.' + @TableName + '_Localization (ObjectId,[Language]) ON [PRIMARY]')

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
	DECLARE @PrimaryKeyName	NVARCHAR(256)
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

BEGIN

	DECLARE @FkName nvarchar(4000)
	DECLARE @ChildTable nvarchar(4000)
	DECLARE @ParentTable nvarchar(4000)
	DECLARE @ColumnName nvarchar(4000)
	DECLARE @ParentKeyColumn nvarchar(4000)
	DECLARE @MetaClassId int
	--get foreign key details for all Localization tables not in catalog system
	DECLARE LocalizationFKCursor CURSOR READ_ONLY
	 FOR SELECT  f.name AS ForeignKey,
				OBJECT_NAME(f.parent_object_id) AS TableName,
				COL_NAME(fc.parent_object_id,
				fc.parent_column_id) AS ColumnName,
				OBJECT_NAME (f.referenced_object_id) AS ReferenceTableName,
				COL_NAME(fc.referenced_object_id,
				fc.referenced_column_id) AS ReferenceColumnName,
				m.MetaClassId AS MetaClassId
		 FROM sys.foreign_keys AS f
	INNER JOIN sys.foreign_key_columns AS fc ON f.OBJECT_ID = fc.constraint_object_id
	INNER JOIN MetaClass M ON M.TableName = replace(OBJECT_NAME(f.parent_object_id),'_Localization','')
	INNER JOIN MetaClass P ON M.ParentClassId = P.MetaClassId
	WHERE OBJECT_NAME(f.parent_object_id) like '%_Localization'
	AND P.TableName not in('CatalogEntry','CatalogNode')
	AND M.Namespace <> 'Mediachase.Commerce.Orders.System'
	AND M.IsSystem = 0
	ORDER by TableName

	OPEN LocalizationFKCursor
	FETCH NEXT FROM LocalizationFKCursor into @FkName,@ChildTable ,@ColumnName,@ParentTable,@ParentKeyColumn,@MetaClassId
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		    
			declare @fk_drop_sql nvarchar(4000) = 'ALTER TABLE '+ @ChildTable + ' DROP CONSTRAINT '+ @FkName
			declare @fk_sql nvarchar(4000) =
						'alter table [dbo].[' + @ChildTable  + '] add constraint [' + @FkName + '] 
							foreign key ('+ @ColumnName + ') references [dbo].[' + @ParentTable + '] ([' + @ParentKeyColumn + ']) on delete cascade on update cascade'
						
			execute dbo.sp_executesql @fk_drop_sql									
			execute dbo.sp_executesql @fk_sql
					
			
	FETCH NEXT FROM LocalizationFKCursor into @FkName,@ChildTable ,@ColumnName,@ParentTable,@ParentKeyColumn,@MetaClassId
	END
	CLOSE LocalizationFKCursor
	DEALLOCATE 	LocalizationFKCursor

END

GO 

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_LoadDictionarySingleItemUsages]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_LoadDictionarySingleItemUsages]
GO

CREATE PROCEDURE dbo.[mdpsp_sys_LoadDictionarySingleItemUsages]
@MetaFieldId int,
@MetaDictionaryId int
as
begin

DECLARE @metaClassTableName nvarchar(256)
DECLARE @sqlQuery nvarchar(max)
DECLARE @metaFieldName nvarchar(256)
DECLARE @multipleLanguage bit
DECLARE @rowcount int

SET @metaFieldName = (SELECT top 1 Name from dbo.MetaField where MetaFieldId = @MetaFieldId)
SET @multipleLanguage = (SELECT top 1 MultiLanguageValue from dbo.MetaField where MetaFieldId = @MetaFieldId)

DECLARE metaclass_table CURSOR FOR 
SELECT TableName
FROM dbo.MetaClass m
INNER JOIN dbo.MetaClassMetaFieldRelation r
ON m.MetaClassId = r.MetaClassId
WHERE r.MetaFieldId = @MetaFieldId

SET @sqlQuery = ''
SET @rowcount = 0

OPEN metaclass_table

FETCH NEXT FROM metaclass_table 
INTO @metaClassTableName


WHILE @@FETCH_STATUS = 0
BEGIN
	if (@multipleLanguage = 1)
		SET @metaClassTableName = @metaClassTableName + '_Localization'
	SET @sqlQuery = 'SELECT @rowcount = Count(ObjectId) FROM ' + @metaClassTableName + ' where ''' +  @metaFieldName +  ''' =  ''' + cast(@MetaDictionaryId as varchar(20)) + ''''
	EXEC sp_executesql @sqlQuery, N'@rowcount int output', @rowcount output
	if (@rowcount > 0)
		BREAK

FETCH NEXT FROM metaclass_table 
    INTO @metaClassTableName

	
END 
CLOSE metaclass_table;
DEALLOCATE metaclass_table;

SELECT @rowcount

END

GO
 
--beginUpdatingDatabaseVersion 
 
INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(5, 6, 1, GETUTCDATE()) 
 
GO 

--endUpdatingDatabaseVersion 

 
