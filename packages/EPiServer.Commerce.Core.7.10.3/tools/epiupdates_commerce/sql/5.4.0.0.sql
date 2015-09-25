--beginvalidatingquery 
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion') 
    BEGIN 
    declare @major int = 5, @minor int = 4, @patch int = 0    
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch) 
        select 0,'Already correct database version' 
    ELSE 
        select 1, 'Upgrading database' 
    END 
ELSE 
    select -1, 'Not an EPiServer Commerce database' 
--endvalidatingquery 
 
GO
CREATE TABLE [dbo].[GuidCatalogMapping](
	[ContentGuid] [uniqueidentifier] NOT NULL,
	[CatalogId] [int] NOT NULL,
	CONSTRAINT [PK_GuidCatalogMapping] PRIMARY KEY NONCLUSTERED([ContentGuid]),
	CONSTRAINT [FK_GuidCatalogMapping_Catalog] FOREIGN KEY ([CatalogId]) REFERENCES [dbo].[Catalog] ([CatalogId]) ON DELETE CASCADE
	)

GO

CREATE CLUSTERED INDEX [IX_GuidCatalogMapping_CatalogId] ON [dbo].[GuidCatalogMapping] ([CatalogId])
GO

CREATE TABLE [dbo].[GuidNodeMapping](
	[ContentGuid] [uniqueidentifier] NOT NULL,
	[CatalogNodeId] [int] NOT NULL,
	CONSTRAINT [PK_GuidNodeMapping] PRIMARY KEY NONCLUSTERED(ContentGuid),
	CONSTRAINT [FK_GuidNodeMapping_CatalogNode] FOREIGN KEY ([CatalogNodeId]) REFERENCES [dbo].[CatalogNode] ([CatalogNodeId]) ON DELETE CASCADE
	)

GO
CREATE CLUSTERED INDEX [IX_GuidNodeMapping_CatalogNodeId] ON [dbo].[GuidNodeMapping] ([CatalogNodeId])
GO


CREATE TABLE [dbo].[GuidEntryMapping](
	[ContentGuid] [uniqueidentifier] NOT NULL,
	[CatalogEntryId] [int] NOT NULL,
	CONSTRAINT [PK_GuidEntryMapping] PRIMARY KEY NONCLUSTERED(ContentGuid),
	CONSTRAINT [FK_GuidEntryMapping_CatalogEntry] FOREIGN KEY ([CatalogEntryId]) REFERENCES [dbo].[CatalogEntry] ([CatalogEntryId]) ON DELETE CASCADE
)
GO

CREATE CLUSTERED INDEX [IX_GuidEntryMapping_CatalogEntryId] ON [dbo].[GuidEntryMapping] ([CatalogEntryId])
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_GuidMappingCatalog_Insert]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_GuidMappingCatalog_Insert]
GO

CREATE PROCEDURE [dbo].[ecf_GuidMappingCatalog_Insert]
	@ContentGuid uniqueidentifier,
	@CatalogId int
AS
BEGIN
	DELETE 
	FROM dbo.GuidCatalogMapping 
	WHERE CatalogId = @CatalogId 
	
	INSERT INTO dbo.GuidCatalogMapping (ContentGuid, CatalogId)
	VALUES (@ContentGuid, @CatalogId)	
END
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_GuidMappingNode_Insert]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_GuidMappingNode_Insert]
GO

CREATE PROCEDURE [dbo].[ecf_GuidMappingNode_Insert]
	@ContentGuid uniqueidentifier,
	@CatalogNodeId int
AS
BEGIN
	DELETE 
	FROM dbo.GuidNodeMapping 
	WHERE CatalogNodeId = @CatalogNodeId
	
	INSERT INTO dbo.GuidNodeMapping (ContentGuid, CatalogNodeId)
	VALUES (@ContentGuid, @CatalogNodeId)	
END
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_GuidMappingEntry_Insert]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_GuidMappingEntry_Insert]
GO

CREATE PROCEDURE [dbo].[ecf_GuidMappingEntry_Insert]
	@ContentGuid uniqueidentifier,
	@CatalogEntryId int
AS
BEGIN
	DELETE 
	FROM dbo.GuidEntryMapping 
	WHERE CatalogEntryId = @CatalogEntryId 
	
	INSERT INTO dbo.GuidEntryMapping (ContentGuid, CatalogEntryId)
	VALUES (@ContentGuid, @CatalogEntryId)	
END
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_GuidMappingCatalog_FindGuid]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_GuidMappingCatalog_FindGuid]
GO
CREATE PROCEDURE [dbo].[ecf_GuidMappingCatalog_FindGuid]
   @CatalogEntityId int
AS
BEGIN
	SELECT ContentGuid
		FROM dbo.GuidCatalogMapping
	WHERE CatalogId = @CatalogEntityId
	
END
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_GuidMappingNode_FindGuid]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_GuidMappingNode_FindGuid]
GO
CREATE PROCEDURE [dbo].[ecf_GuidMappingNode_FindGuid]
    @CatalogEntityId int
AS
BEGIN
	SELECT ContentGuid
		FROM dbo.GuidNodeMapping
	WHERE CatalogNodeId = @CatalogEntityId
	
END
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_GuidMappingEntry_FindGuid]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_GuidMappingEntry_FindGuid]
GO
CREATE PROCEDURE [dbo].[ecf_GuidMappingEntry_FindGuid]
    @CatalogEntityId int
AS
BEGIN
	SELECT ContentGuid
		FROM dbo.GuidEntryMapping
	WHERE CatalogEntryId = @CatalogEntityId
END
GO


IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_GuidMapping_FindEntity]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_GuidMapping_FindEntity]
GO

CREATE PROCEDURE [dbo].[ecf_GuidMapping_FindEntity]
    @ContentGuid uniqueidentifier
AS
BEGIN
	SELECT CatalogId,2
		FROM dbo.GuidCatalogMapping
	WHERE ContentGuid = @ContentGuid
	UNION
	SELECT CatalogNodeId,1
		FROM dbo.GuidNodeMapping
	WHERE ContentGuid = @ContentGuid
	UNION
	SELECT CatalogEntryId,0
		FROM dbo.GuidEntryMapping
	WHERE ContentGuid = @ContentGuid

END

GO


IF OBJECT_ID ('GuidMapping_CatalogInsert', 'TR') IS NOT NULL
   DROP TRIGGER GuidMapping_CatalogInsert;
GO
CREATE TRIGGER [GuidMapping_CatalogInsert] ON Catalog
    FOR INSERT 
	AS
	BEGIN
		IF NOT EXISTS( SELECT g.CatalogId FROM [dbo].[GuidCatalogMapping] g INNER JOIN inserted i on g.CatalogId = i.CatalogId)
		BEGIN
			INSERT INTO [dbo].[GuidCatalogMapping] (ContentGuid, CatalogId)
			SELECT NEWID(), CatalogId FROM inserted
		END
END
GO


IF OBJECT_ID ('GuidMapping_EntryInsert', 'TR') IS NOT NULL
   DROP TRIGGER GuidMapping_EntryInsert;
GO
CREATE TRIGGER [GuidMapping_EntryInsert] ON CatalogEntry 
    FOR INSERT 
	AS
	BEGIN
		IF NOT EXISTS( SELECT g.CatalogEntryId FROM [dbo].[GuidEntryMapping] g INNER JOIN inserted i on g.CatalogEntryId = i.CatalogEntryId)
		BEGIN
			INSERT INTO [dbo].[GuidEntryMapping] (ContentGuid, CatalogEntryId)
			SELECT NEWID(), CatalogEntryId FROM inserted
		END
	END
GO

IF OBJECT_ID ('GuidMapping_NodeInsert', 'TR') IS NOT NULL
   DROP TRIGGER GuidMapping_NodeInsert;
GO
CREATE TRIGGER [GuidMapping_NodeInsert] ON CatalogNode 
    FOR INSERT
	AS
	BEGIN
		IF NOT EXISTS( SELECT g.CatalogNodeId FROM [dbo].[GuidNodeMapping] g INNER JOIN inserted i on g.CatalogNodeId = i.CatalogNodeId)
		BEGIN
			INSERT INTO [dbo].[GuidNodeMapping] (ContentGuid, CatalogNodeId)
			SELECT NEWID(), CatalogNodeId FROM inserted
		END
	END
GO



IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_EncodeGuid]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT')) DROP FUNCTION [dbo].[ecf_EncodeGuid]
GO

CREATE FUNCTION  dbo.ecf_EncodeGuid 
(@objectId int, @contenttype int, @ProviderNameHash binary(4) = 0x66799143)
  RETURNS uniqueidentifier
  AS
  BEGIN
  DECLARE @baseguid uniqueidentifier;
  DECLARE @baseguidbinary binary(16)
  DECLARE @contentId int;
  DECLARE @DataToEncode binary(8);
  DECLARE @first binary(8)
  DECLARE @last binary(8)
  DECLARE @result binary(16)

  --predefined guid
  SET @baseguid = '4b37b783-54b4-4011-a395-65a7165539b7'
  SET @baseguidbinary = CAST(@baseguid AS binary(16))

  --T-SQL only supports XOR on bigint or smaller number. 
  --We need to devide guid to two parts so we can do the xor
  SET @first = SUBSTRING(@baseguidbinary, 1, 8)
  SET @last = SUBSTRING(@baseguidbinary, 9, 8)

  --get the contentId from the objectId and the contenttype. the contenttype is shifted 30 bit
  if (@contenttype = 1)  
  BEGIN
  SET @contenttype = 0x40000000
    END
  else if (@contenttype = 2)
    begin
  SET @contenttype = 0x80000000
    END

  SET @contentId = @objectId | @contenttype

  --get the data we want to store
  --Due to use of BlockCopy in cs code, we need to reverse the int(s) we want to store
  SET @DataToEncode = cast(REVERSE(cast(@contentid as binary(4))) as binary(4)) + cast (0 as binary(4)) 

  --XOR can only work with one of two operand is binary. Another one must be converted.
  SET @first = @first  ^ cast(@DataToEncode as bigint)
 
  SET @DataToEncode =  @ProviderNameHash + cast (0 as binary(4))

  SET @last = @last ^ cast(@DataToEncode as bigint)


  return convert(uniqueidentifier, @first + @last)
  END


GO


-- Insert data into GuidMapping table
INSERT INTO dbo.GuidCatalogMapping(ContentGuid, CatalogId)
(SELECT dbo.ecf_EncodeGuid(c.CatalogId, 2, default), c.CatalogId FROM dbo.Catalog c)

INSERT INTO dbo.GuidNodeMapping(ContentGuid, CatalogNodeId)
(SELECT dbo.ecf_EncodeGuid(n.CatalogNodeId, 1, default), n.CatalogNodeId FROM dbo.CatalogNode n)

INSERT INTO dbo.GuidEntryMapping(ContentGuid, CatalogEntryId)
(SELECT dbo.ecf_EncodeGuid(e.CatalogEntryId, 0, default), e.CatalogEntryId FROM dbo.CatalogEntry e)

GO

--beginUpdatingDatabaseVersion

INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(5, 4, 0, GETUTCDATE())

GO

--endUpdatingDatabaseVersion