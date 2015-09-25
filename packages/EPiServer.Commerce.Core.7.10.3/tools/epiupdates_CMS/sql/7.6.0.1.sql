--beginvalidatingquery
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'tblContentType') 
    BEGIN 
	IF EXISTS (SELECT pkid FROM dbo.tblContentType WHERE ModelType LIKE 'EPiServer.Business.Commerce.ContentProviders.CommerceRootContent, EPiServer.Business.Commerce, Version=7.0.243.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7')
		SELECT 1, 'Migrating Commerce Content Root'
	ELSE
		SELECT 0, 'Already migrated Commerce Content Root'
    END 
ELSE 
    select -1, 'Not an EPiServer CMS database' 
--endvalidatingquery

GO

-- if RootContent is not available, we need to visit the site to add it to tblContentType table.
IF NOT EXISTS (SELECT pkid FROM dbo.tblContentType WHERE ModelType LIKE 'EPiServer.Commerce.Catalog.ContentTypes.RootContent%')
	RAISERROR ('RootContent type not found. Please add it to the system by visiting the website, then try again with the sql update.', 10, 1);
GO

UPDATE [dbo].[tblContent] 
SET fkContentTypeID = (SELECT pkid FROM dbo.tblContentType WHERE ModelType LIKE 'EPiServer.Commerce.Catalog.ContentTypes.RootContent%') 
WHERE fkContentTypeID = (SELECT pkid FROM dbo.tblContentType WHERE ModelType = 'EPiServer.Business.Commerce.ContentProviders.CommerceRootContent, EPiServer.Business.Commerce, Version=7.0.243.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7')

GO