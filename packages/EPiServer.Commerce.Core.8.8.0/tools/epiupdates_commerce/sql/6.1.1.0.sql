--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 6, @minor int = 1, @patch int = 1
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
GO

IF ((SELECT azureCompatible FROM dbo.AzureCompatible) = 1)
BEGIN
    SET NOEXEC ON
END
GO

--Skip executing script in this block on Azure
-----------------------------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_RotateEncryptionKeys]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_RotateEncryptionKeys]
GO

CREATE PROCEDURE [dbo].[mdpsp_sys_RotateEncryptionKeys] AS
DECLARE @Query_tmp  nvarchar(max)

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
BEGIN TRANSACTION

DECLARE @MetaClassTable NVARCHAR(256), @MetaFieldName NVARCHAR(256), @MultiLanguageValue BIT
DECLARE @sqlQuery NVARCHAR(4000)
DECLARE classall_cursor CURSOR FOR
	SELECT MF.Name, MF.MultiLanguageValue, MC.TableName FROM MetaField MF
		INNER JOIN MetaClassMetaFieldRelation MCFR ON MCFR.MetaFieldId = MF.MetaFieldId
		INNER JOIN MetaClass MC ON MC.MetaClassId = MCFR.MetaClassId
		WHERE MF.IsEncrypted = 1 AND MC.IsSystem = 0

--Open symmetric key
exec mdpsp_sys_OpenSymmetricKey

OPEN classall_cursor
	FETCH NEXT FROM classall_cursor INTO @MetaFieldName, @MultiLanguageValue, @MetaClassTable

--Decrypt meta values
WHILE(@@FETCH_STATUS = 0)
BEGIN

	IF @MultiLanguageValue = 0
		SET @Query_tmp = '
			UPDATE '+@MetaClassTable+'
				SET ['+@MetaFieldName+'] = dbo.mdpfn_sys_EncryptDecryptString(['+@MetaFieldName+'], 0)
				WHERE NOT [' + @MetaFieldName + '] IS NULL'
	ELSE
		SET @Query_tmp = '
			UPDATE '+@MetaClassTable+'_Localization
				SET ['+@MetaFieldName+'] = dbo.mdpfn_sys_EncryptDecryptString(['+@MetaFieldName+'], 0)
				WHERE NOT [' + @MetaFieldName + '] IS NULL'

	EXEC(@Query_tmp)

	IF @@ERROR <> 0 GOTO ERR

	FETCH NEXT FROM classall_cursor INTO @MetaFieldName, @MultiLanguageValue, @MetaClassTable
END

CLOSE classall_cursor

--Decrypt credit cards
SET @sqlQuery = 'UPDATE dbo.cls_CreditCard
SET [CreditCardNumber] = CCD.CardNumber_string,
[SecurityCode] = CCD.SecurityCode_string
FROM (SELECT CONVERT(VARCHAR(max), DecryptByKey(cast(N'''' AS XML).value(''xs:base64Binary(sql:column("CC.CreditCardNumber"))'', ''varbinary(max)''))) AS [CardNumber_string],
    CONVERT(VARCHAR(max), DecryptByKey(cast(N'''' AS XML).value(''xs:base64Binary(sql:column("CC.SecurityCode"))'',''varbinary(max)''))) AS [SecurityCode_string],
    CreditCardId
FROM cls_CreditCard CC WHERE CC.CreditCardNumber is not NULL) CCD WHERE CCD.CreditCardId = cls_CreditCard.CreditCardId'

EXECUTE sp_executesql @sqlQuery

--Close symmetric key
exec mdpsp_sys_CloseSymmetricKey

--Recreate symmetric key
SET @sqlQuery = ' 
DROP SYMMETRIC KEY Mediachase_ECF50_MDP_Key
CREATE SYMMETRIC KEY Mediachase_ECF50_MDP_Key
WITH ALGORITHM = AES_128 ENCRYPTION BY CERTIFICATE Mediachase_ECF50_MDP'
EXECUTE sp_executesql @sqlQuery

--Open new symmetric key
exec mdpsp_sys_OpenSymmetricKey

OPEN classall_cursor
	FETCH NEXT FROM classall_cursor INTO @MetaFieldName, @MultiLanguageValue, @MetaClassTable

--Encrypt meta values
WHILE(@@FETCH_STATUS = 0)
BEGIN

	IF @MultiLanguageValue = 0
		SET @Query_tmp = '
			UPDATE '+@MetaClassTable+'
				SET ['+@MetaFieldName+'] = dbo.mdpfn_sys_EncryptDecryptString(['+@MetaFieldName+'], 1)
				WHERE NOT [' + @MetaFieldName + '] IS NULL'
	ELSE
		SET @Query_tmp = '
			UPDATE '+@MetaClassTable+'_Localization
				SET ['+@MetaFieldName+'] = dbo.mdpfn_sys_EncryptDecryptString(['+@MetaFieldName+'], 1)
				WHERE NOT [' + @MetaFieldName + '] IS NULL'

	EXEC(@Query_tmp)

	FETCH NEXT FROM classall_cursor INTO @MetaFieldName, @MultiLanguageValue, @MetaClassTable
END

CLOSE classall_cursor
DEALLOCATE classall_cursor

--Encrypt credit cards
SET @sqlQuery = 'UPDATE  cls_CreditCard
SET CreditCardNumber = CONVERT(nvarchar(512), CAST(N'''' AS xml).value(''xs:base64Binary(sql:column("CC.CreditCardNumber_string"))'', ''varchar(4000)'') ) ,
    SecurityCode = CONVERT(nvarchar(255), CAST(N'''' AS xml).value(''xs:base64Binary(sql:column("CC.SecurityCode_string"))'', ''varchar(4000)'') ) 
FROM
    ( SELECT EncryptByKey(Key_GUID(''Mediachase_ECF50_MDP_Key''), (CONVERT(varchar(4000), CreditCardNumber)))  CreditCardNumber_string
        , EncryptByKey(Key_GUID(''Mediachase_ECF50_MDP_Key''), (CONVERT(varchar(4000), SecurityCode)))  SecurityCode_string
        , CreditCardId FROM [cls_CreditCard]) CC WHERE cls_CreditCard.CreditCardId = CC.CreditCardId'
EXECUTE sp_executesql @sqlQuery

--Close new symmetric key
exec mdpsp_sys_CloseSymmetricKey

COMMIT TRAN
RETURN

ERR:
	ROLLBACK TRAN
RETURN
GO

SET NOEXEC OFF
GO

--Continue to execute even on Azure
-----------------------------------------------------------

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[mdpsp_sys_CreateMetaClassProcedure]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[mdpsp_sys_CreateMetaClassProcedure]
GO

create procedure [dbo].[mdpsp_sys_CreateMetaClassProcedure]
    @MetaClassId int
as
begin
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
            set @ColumnReadBase = case when @azureCompatible <> 1 and @FieldIsEncrypted = 1 then 'dbo.mdpfn_sys_EncryptDecryptString(T.[' + @ColumnName + '],0)' + ' as [' + @ColumnName + ']' else 'T.[' + @ColumnName + ']' end
            set @ColumnReadLocal = case when @azureCompatible <> 1 and @FieldIsEncrypted = 1 then 'dbo.mdpfn_sys_EncryptDecryptString(L.[' + @ColumnName + '],0)' + ' as [' + @ColumnName + ']' else 'L.[' + @ColumnName + ']' end
            set @WriteValue = case when @azureCompatible <> 1 and @FieldIsEncrypted = 1 then 'dbo.mdpfn_sys_EncryptDecryptString(' + @ParameterName + ',1)' else @ParameterName end

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
                    where cfr.MetaClassId = @MetaClassId and mf.SystemMetaClassId = 0 and mf.IsEncrypted = 1) and @azureCompatible <> 1
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
end

GO

--beginUpdatingDatabaseVersion 
 
INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(6, 1, 1, GETUTCDATE()) 
GO 

--endUpdatingDatabaseVersion 
