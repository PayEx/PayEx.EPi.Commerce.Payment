--beginvalidatingquery
IF EXISTS (SELECT 1 FROM sys.views WHERE Name = 'VW_EPiServer.Shell.Storage.ComponentData') 
    BEGIN 
	IF EXISTS (SELECT Id FROM dbo.[VW_EPiServer.Shell.Storage.ComponentData] WHERE DefinitionName LIKE 'EPiServer.Commerce.AddOns.UI%')
		SELECT 1, 'Migrating Commerce Component Definition Name'
	ELSE
		SELECT 0, 'Already migrated Commerce Component Definition Name'
    END 
ELSE 
    select 0, 'No component needs to be updated' 
--endvalidatingquery

-- Migrate Component Definition Name since EPiServer.Commerce.AddOns.UI has been renamed to EPiServer.Commerce.Shell.
-- This SQL can be run on a fresh CMS database, the redundant check here make sure it will not throw any error.
IF EXISTS (SELECT 1 FROM sys.views WHERE Name = 'VW_EPiServer.Shell.Storage.ComponentData') 
BEGIN 
UPDATE dbo.[VW_EPiServer.Shell.Storage.ComponentData]
SET DefinitionName = REPLACE(DefinitionName, 'EPiServer.Commerce.AddOns.UI', 'EPiServer.Commerce.Shell') 
WHERE DefinitionName LIKE 'EPiServer.Commerce.AddOns.UI%'
END

GO
