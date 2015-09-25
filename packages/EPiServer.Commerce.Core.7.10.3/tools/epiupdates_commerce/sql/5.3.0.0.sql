--beginvalidatingquery
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion')
    BEGIN
    declare @major int = 5, @minor int = 3, @patch int = 0
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch)
        select 0,'Already correct database version'
    ELSE
        select 1, 'Upgrading database'
    END
ELSE
    select -1, 'Not an EPiServer Commerce database'
--endvalidatingquery

GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_LoadDictionaryMultiItemUsages]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_LoadDictionaryMultiItemUsages]
GO

CREATE PROCEDURE [dbo].[mdpsp_sys_LoadDictionaryMultiItemUsages]
(
    @MetaDictionaryId   int
)
AS
BEGIN
  SELECT COUNT(MetaObjectId) from metakey mk
  join MetaMultiValueDictionary d on mk.MetaKey = d.MetaKey
  where d.MetaDictionaryId = @MetaDictionaryId
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
    SET @sqlQuery = 'SELECT @rowcount = Count(ObjectId) FROM ' + @metaClassTableName + ' where ' +  @metaFieldName +  ' =  ' + cast(@MetaDictionaryId as varchar(20))
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

INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(5, 3, 0, GETUTCDATE())

GO

--endUpdatingDatabaseVersion
