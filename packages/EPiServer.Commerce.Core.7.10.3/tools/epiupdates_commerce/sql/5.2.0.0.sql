--beginvalidatingquery
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SchemaVersion')
    BEGIN
    declare @major int = 5, @minor int = 2, @patch int = 0
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE Major = @major AND Minor = @minor AND Patch = @patch)
        select 0,'Already correct database version'
    ELSE
        select 1, 'Upgrading database'
    END
ELSE
    select -1, 'Not an EPiServer Commerce database'
--endvalidatingquery

GO

-- Start bug 114126
CREATE TABLE [dbo].[MarketShippingMethods] (
    [MarketId]        NVARCHAR (8)     NOT NULL,
    [ShippingMethodId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_MarketShippingMethods] PRIMARY KEY CLUSTERED ([MarketId] ASC, [ShippingMethodId] ASC),
    CONSTRAINT [FK_MarketShippingMethods_Market] FOREIGN KEY ([MarketId]) REFERENCES [dbo].[Market] ([MarketId]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_MarketShippingMethods_PaymentMethod] FOREIGN KEY ([ShippingMethodId]) REFERENCES [dbo].[ShippingMethod] ([ShippingMethodId]) ON DELETE CASCADE ON UPDATE CASCADE
)
GO
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_ShippingMethod_Language]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_ShippingMethod_Language]
GO
CREATE PROCEDURE [dbo].[ecf_ShippingMethod_Language]
	@ApplicationId uniqueidentifier,
	@LanguageId nvarchar(10) = null,
	@ReturnInactive bit = 0
AS
BEGIN
	select * from [ShippingOption] where [ApplicationId] = @ApplicationId
	select SOP.* from [ShippingOptionParameter] SOP
	inner join [ShippingOption] SO on SOP.[ShippingOptionId]=SO.[ShippingOptionId]
		where SO.[ApplicationId] = @ApplicationId
	select distinct SM.* from [ShippingMethod] SM
	inner join [Warehouse] W on SM.ApplicationId = W.ApplicationId
		where COALESCE(@LanguageId, LanguageId) = LanguageId and ((SM.[IsActive] = 1) or @ReturnInactive = 1) and SM.ApplicationId = @ApplicationId
			and (SM.Name <> 'In Store Pickup' or W.IsPickupLocation = 1)
	select * from [ShippingMethodParameter] where ShippingMethodId in (select ShippingMethodId from ShippingMethod where COALESCE(@LanguageId, LanguageId) = LanguageId and (([IsActive] = 1) or @ReturnInactive = 1) and ApplicationId = @ApplicationId)
	select * from [ShippingMethodCase] where ShippingMethodId in (select ShippingMethodId from ShippingMethod where COALESCE(@LanguageId, LanguageId) = LanguageId and (([IsActive] = 1) or @ReturnInactive = 1) and ApplicationId = @ApplicationId)
	select * from [ShippingCountry] where ShippingMethodId in (select ShippingMethodId from ShippingMethod where COALESCE(@LanguageId, LanguageId) = LanguageId and (([IsActive] = 1) or @ReturnInactive = 1) and ApplicationId = @ApplicationId)
	select * from [ShippingRegion] where ShippingMethodId in (select ShippingMethodId from ShippingMethod where COALESCE(@LanguageId, LanguageId) = LanguageId and (([IsActive] = 1) or @ReturnInactive = 1) and ApplicationId = @ApplicationId)
	select * from [ShippingPaymentRestriction]
		where
			(ShippingMethodId in (select ShippingMethodId from ShippingMethod where COALESCE(@LanguageId, LanguageId) = LanguageId and (([IsActive] = 1) or @ReturnInactive = 1) and ApplicationId = @ApplicationId) )
				and
			[RestrictShippingMethods] = 0
	select * from [Package] where [ApplicationId] = @ApplicationId
	select SP.* from [ShippingPackage] SP
	inner join [Package] P on SP.[PackageId]=P.[PackageId]
		where P.[ApplicationId] = @ApplicationId
	select * from [MarketShippingMethods] where ShippingMethodId in (select ShippingMethodId from ShippingMethod where COALESCE(@LanguageId, LanguageId) = LanguageId and (([IsActive] = 1) or @ReturnInactive = 1) and ApplicationId = @ApplicationId)
END
GO
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_ShippingMethod_Market]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_ShippingMethod_Market]
GO
CREATE PROCEDURE [dbo].[ecf_ShippingMethod_Market]
    @ApplicationId uniqueidentifier,
    @MarketId nvarchar(10) = null,
    @ReturnInactive bit = 0
AS
BEGIN
    declare @_shippingMethodIds as table (ShippingMethodId uniqueidentifier)
    insert into @_shippingMethodIds
    select SM.ShippingMethodId
        from [ShippingMethod] SM
        inner join [MarketShippingMethods] MSM
          on SM.ShippingMethodId = MSM.ShippingMethodId
        inner join [Warehouse] W
          on W.ApplicationId = SM.ApplicationId
        where COALESCE(@MarketId, MSM.MarketId) = MSM.MarketId
          and ((SM.[IsActive] = 1) or (@ReturnInactive = 1))
          and SM.ApplicationId = @ApplicationId
          and (SM.Name <> 'In Store Pickup' or W.IsPickupLocation = 1)
    select * from [ShippingOption] where [ApplicationId] = @ApplicationId

    select SOP.* from [ShippingOptionParameter] SOP
    inner join [ShippingOption] SO on SOP.[ShippingOptionId]=SO.[ShippingOptionId]
        where SO.[ApplicationId] = @ApplicationId

    select distinct SM.* from [ShippingMethod] SM where ShippingMethodId in (select ShippingMethodId from @_shippingMethodIds)
    select * from [ShippingMethodParameter] where ShippingMethodId in (select ShippingMethodId from @_shippingMethodIds)
    select * from [ShippingMethodCase] where ShippingMethodId in (select ShippingMethodId from @_shippingMethodIds)
    select * from [ShippingCountry] where ShippingMethodId in (select ShippingMethodId from @_shippingMethodIds)
    select * from [ShippingRegion] where ShippingMethodId in (select ShippingMethodId from @_shippingMethodIds)

    select * from [ShippingPaymentRestriction]
        where
            ShippingMethodId in (select ShippingMethodId from @_shippingMethodIds)
            and
            [RestrictShippingMethods] = 0
    select * from [Package] where [ApplicationId] = @ApplicationId
    select SP.* from [ShippingPackage] SP
    inner join [Package] P on SP.[PackageId]=P.[PackageId]
        where P.[ApplicationId] = @ApplicationId
	select * from [MarketShippingMethods] where ShippingMethodId in (select ShippingMethodId from @_shippingMethodIds)
END
GO
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id (N'[dbo].[ecf_ShippingMethod_ShippingMethodId]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) DROP PROCEDURE [dbo].[ecf_ShippingMethod_ShippingMethodId]
GO
CREATE PROCEDURE [dbo].[ecf_ShippingMethod_ShippingMethodId]
	@ApplicationId uniqueidentifier,
	@ShippingMethodId uniqueidentifier,
	@ReturnInactive bit = 0
AS
BEGIN
	select SO.* from [ShippingOption] SO
		inner join [ShippingMethod] SM on SO.[ShippingOptionId]=SM.[ShippingOptionId]
	where SM.[ShippingMethodId] = @ShippingMethodId and SM.[ApplicationId] = @ApplicationId
	select SOP.* from [ShippingOptionParameter] SOP
		inner join [ShippingMethod] SM on SOP.[ShippingOptionId]=SM.[ShippingOptionId]
	where SM.[ShippingMethodId] = @ShippingMethodId and SM.[ApplicationId] = @ApplicationId
	select SM.* from [ShippingMethod] SM
		where ((SM.[IsActive] = 1) or @ReturnInactive = 1) and SM.[ApplicationId] = @ApplicationId and SM.[ShippingMethodId] = @ShippingMethodId
	select SMP.* from [ShippingMethodParameter] SMP
		inner join [ShippingMethod] SM on SMP.[ShippingMethodId]=SM.[ShippingMethodId]
			where ((SM.[IsActive] = 1) or @ReturnInactive = 1) and SM.[ApplicationId] = @ApplicationId and SM.[ShippingMethodId] = @ShippingMethodId
	select SMC.* from [ShippingMethodCase] SMC
		inner join [ShippingMethod] SM on SMC.[ShippingMethodId]=SM.[ShippingMethodId]
			where ((SM.[IsActive] = 1) or @ReturnInactive = 1) and SM.[ApplicationId] = @ApplicationId and SM.[ShippingMethodId] = @ShippingMethodId
	select SC.* from [ShippingCountry] SC
		inner join [ShippingMethod] SM on SC.[ShippingMethodId]=SM.[ShippingMethodId]
			where ((SM.[IsActive] = 1) or @ReturnInactive = 1) and SM.[ApplicationId] = @ApplicationId and SM.[ShippingMethodId] = @ShippingMethodId
	select SR.* from [ShippingRegion] SR
		inner join [ShippingMethod] SM on SR.[ShippingMethodId]=SM.[ShippingMethodId]
			where ((SM.[IsActive] = 1) or @ReturnInactive = 1) and SM.[ApplicationId] = @ApplicationId and SM.[ShippingMethodId] = @ShippingMethodId
	select SPR.* from [ShippingPaymentRestriction] SPR
		inner join [ShippingMethod] SM on SPR.[ShippingMethodId]=SM.[ShippingMethodId]
			where ((SM.[IsActive] = 1) or @ReturnInactive = 1) and SM.[ApplicationId] = @ApplicationId and SM.[ShippingMethodId] = @ShippingMethodId and SPR.[RestrictShippingMethods] = 0
	select P.* from [Package] P
		inner join [ShippingPackage] SP on SP.[PackageId]=P.[PackageId]
		inner join [ShippingMethod] SM on SP.[ShippingOptionId]=SM.[ShippingOptionId]
			where ((SM.[IsActive] = 1) or @ReturnInactive = 1) and SM.[ApplicationId] = @ApplicationId and SM.[ShippingMethodId] = @ShippingMethodId
	select SP.* from [ShippingPackage] SP
		inner join [ShippingMethod] SM on SP.[ShippingOptionId]=SM.[ShippingOptionId]
			where ((SM.[IsActive] = 1) or @ReturnInactive = 1) and SM.[ApplicationId] = @ApplicationId and SM.[ShippingMethodId] = @ShippingMethodId
	select SMS.* from [MarketShippingMethods] SMS
		inner join [ShippingMethod] SM on SMS.[ShippingMethodId]=SM.[ShippingMethodId]
			where ((SM.[IsActive] = 1) or @ReturnInactive = 1) and SM.[ApplicationId] = @ApplicationId and SM.[ShippingMethodId] = @ShippingMethodId
END
GO
INSERT INTO [dbo].[MarketShippingMethods] ([MarketId], [ShippingMethodId])
	Select M.MarketId, SM.ShippingMethodId From ShippingMethod SM
		inner join [MarketLanguages] ML on SM.LanguageId = ML.LanguageCode
		inner join [Market] M on ML.MarketId = M.MarketId
		left outer join MarketShippingMethods MSM on MSM.MarketId = M.MarketId AND MSM.ShippingMethodId = SM.ShippingMethodId
	Where MSM.MarketId IS null and MSM.ShippingMethodId IS null
GO
-- End bug 114126

--beginUpdatingDatabaseVersion

INSERT INTO dbo.SchemaVersion(Major, Minor, Patch, InstallDate) VALUES(5, 2, 0, GETUTCDATE())

GO

--endUpdatingDatabaseVersion
