--beginvalidatingquery
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'tblBigTable') 
	BEGIN
	DECLARE @maxVersionAllowed int = 77 
	DECLARE @maxVersion int
	--get the two first significant digits of the dll version before the upgrade
	SET @maxVersion =  (SELECT MAX(DISTINCT    SUBSTRING
							(REPLACE(
								     REPLACE(SUBSTRING(ItemType,CHARINDEX('Version=',ItemType), charindex(',',ItemType,charindex(',',ItemType,CHARINDEX(',',ItemType)+1)+1) -CHARINDEX('Version=',ItemType)) ,
										   'Version=',''),
								   '.',
								   ''),1,2))
								   FROM dbo.tblBigTable
								WHERE ItemType like '%EPiServer.Commerce.Catalog.Provider.CatalogContentDraft%')
		   
	--compares the two first  digits of the old dll version with two first digits of dll version upgraded number 
	IF @maxVersion <= @maxVersionAllowed    
    	SELECT 1, 'Removing catalog node from dds'
	ELSE
		SELECT 0, 'Already removed catalog node from dds'
    END 
ELSE 
    SELECT -1, 'Not an EPiServer CMS database' 
GO
--endvalidatingquery

DECLARE @ShiftRight int
DECLARE @entryUint bigint
DECLARE @pkid int
DECLARE @IndexedInteger bigint

DECLARE db_cursor CURSOR FOR		
SELECT PKID, 
	   Indexed_Integer01
	   FROM dbo.tblBigTable	
	   WHERE (String01 like '%__CatalogContent' or
			String02 like '%__CatalogContent' or
			String02 like '%__CatalogContent' or
			String03 like '%__CatalogContent' or
			String04 like '%__CatalogContent' or
			String05 like '%__CatalogContent' or
			String06 like '%__CatalogContent' or
			String07 like '%__CatalogContent' or
			String08 like '%__CatalogContent' or
			String09 like '%__CatalogContent' or
			String10 like '%__CatalogContent')
	    
OPEN db_cursor
FETCH NEXT FROM db_cursor INTO @pkid,@IndexedInteger

WHILE @@FETCH_STATUS = 0
BEGIN
		--convert to uint
		BEGIN
		IF @IndexedInteger < 0
			SET @entryUint = @IndexedInteger + 4294967296
		ELSE
		    SET @entryUint = @IndexedInteger
		END
		--shift right
		SET @ShiftRight = CONVERT(int,@entryUint/POWER(2,30))
		
		--remove catalog row from dds
		IF @ShiftRight = 2
		BEGIN
		EXEC dbo.BigTableDeleteItem @StoreId= @pkid
		END
		
		FETCH NEXT FROM db_cursor INTO @pkid,@IndexedInteger

END
CLOSE db_cursor
DEALLOCATE db_cursor


GO