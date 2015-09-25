--beginvalidatingquery
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion')
    BEGIN
    declare @major int = 5, @minor int = 5, @patch int = 1
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch)
        select 0,'Already correct database version'
    ELSE
        select 1, 'Upgrading database'
    END
ELSE
    select -1, 'Not an EPiServer Commerce database'
--endvalidatingquery
GO


---re-create foreign keys for localization table to avoid delete cascade
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
	AND M.IsSystem = 0
	ORDER by TableName

	OPEN LocalizationFKCursor
	FETCH NEXT FROM LocalizationFKCursor into @FkName,@ChildTable ,@ColumnName,@ParentTable,@ParentKeyColumn,@MetaClassId
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		    
			declare @fk_drop_sql nvarchar(4000) = 'ALTER TABLE '+ @ChildTable + ' DROP CONSTRAINT '+ @FkName
			declare @fk_sql nvarchar(4000) =
						'alter table [dbo].[' + @ChildTable  + '] add constraint [' + @FkName + '] 
							foreign key ('+ @ColumnName + ') references [dbo].[' + @ParentTable + '] ([' + @ParentKeyColumn + ']) on update cascade'
						
			execute dbo.sp_executesql @fk_drop_sql									
			execute dbo.sp_executesql @fk_sql
					
			
	FETCH NEXT FROM LocalizationFKCursor into @FkName,@ChildTable ,@ColumnName,@ParentTable,@ParentKeyColumn,@MetaClassId
	END
	CLOSE LocalizationFKCursor
	DEALLOCATE 	LocalizationFKCursor

END

GO



--beginUpdatingDatabaseVersion

INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(5, 5, 1, GETUTCDATE())

GO

--endUpdatingDatabaseVersion
