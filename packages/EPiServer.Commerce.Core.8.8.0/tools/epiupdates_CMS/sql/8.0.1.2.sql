--beginvalidatingquery
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'tblContentType') 
    SELECT -1, 'Not an EPiServer CMS database' 
ELSE
	SELECT 1, 'Verifying/creating Campaign root...'
--endvalidatingquery

GO

DECLARE @contentTypeGuid UNIQUEIDENTIFIER
DECLARE @campaignRootGuid UNIQUEIDENTIFIER
DECLARE @campaignRootName varchar(256)
SET @contentTypeGuid = '00C8157D-8117-4D0D-B449-B31960ABA2D4'
SET @campaignRootGuid = '48E4889F-926B-478C-9EAE-25AE12C4AEE2'
SET @campaignRootName = 'SysCampaignRoot'

-- Delete duplicate content types that may have been created by earlier versions of this script
DELETE t FROM tblContentType t
	WHERE t.ContentTypeGUID = @contentTypeGuid
	AND (NOT EXISTS (SELECT * FROM tblContent c WHERE c.fkContentTypeID = t.pkID))

DECLARE @LanguageBranchID INT
SELECT @LanguageBranchID = pkID FROM tblLanguageBranch WHERE LanguageID = ''

-- Insert campaign root content type and content instance if not already existing
DECLARE @fkContentId INT
SELECT @fkContentId = pkID FROM tblContent WHERE ContentGUID = @campaignRootGuid
IF @fkContentId IS NULL
	BEGIN
		DECLARE @fkContentType INT
		SET @fkContentType = (SELECT pkID FROM tblContentType WHERE ContentTypeGUID = @contentTypeGuid)
		IF @fkContentType IS NULL
			INSERT INTO tblContentType (ContentTypeGUID, Created, ModelType, Name, Description, Available, SortOrder, IdString, WorkflowEditFields, ContentType) VALUES (
			'{00C8157D-8117-4D0D-B449-B31960ABA2D4}',
			'19990101 00:00',
			'EPiServer.Core.CampaignFolder,EPiServer',
			'SysCampaignFolder',
			'Used as root of campaigns',
			0,
			10040,
			'?id=',
			0,
			2)
			SET @fkContentType = @@identity

		INSERT INTO tblContent (fkContentTypeID, fkParentID, ContentGUID, VisibleInMenu, ChildOrderRule, PeerOrder, ExternalFolderID, fkMasterLanguageBranchID, ContentPath, ContentType) VALUES (
		@fkContentType,--fkContentTypeID
		1,--fkParentID,
		@campaignRootGuid,
		1,--VisibleInMenu
		3,--ChildOrderRule
		100,--PeerOrder
		0,--ExternalFolderID
		@LanguageBranchID,--fkMasterLanguageBranchID
		'.1.',--ContentPath
		2)--ContentType
		SET @fkContentId = @@IDENTITY

		DECLARE @versionId INT
		INSERT INTO tblWorkContent (fkContentID, fkMasterVersionID, ContentLinkGUID, fkFrameID, ArchiveContentGUID, ChangedByName, NewStatusByName, Name, URLSegment, LinkURL, ExternalURL, VisibleInMenu, LinkType, Created, Saved, StartPublish, StopPublish, ChildOrderRule, PeerOrder, ChangedOnPublish, RejectComment, fkLanguageBranchID, Status) VALUES (
		@fkContentId, --fkContentID
		null, --,fkMasterVersionID
		null, --,ContentLinkGUID
		null,--,fkFrameID
		null,--,ArchiveContentGUID
		'', --,ChangedByName
		null, --,NewStatusByName
		@campaignRootName,--Name
				  
		null, --,URLSegment
		'~/link/' + REPLACE(@campaignRootGuid, '-', '') + '.aspx',--LinkURL
		null, --,ExternalURL
		1, --,VisibleInMenu
		0, --,LinkType
		'19990101 00:00', --,Created
		'19990101 00:00', --,Saved
		'19990101 00:00', --,StartPublish
		null, --,StopPublish
		3, --,ChildOrderRule
		100, --,PeerOrder
		0, --,ChangedOnPublish
		null,--,RejectComment
		@LanguageBranchID,--,fkLanguageBranchID
		4 --status
		)
		SET @versionId=@@IDENTITY

		INSERT INTO tblContentLanguage (fkContentID, fkLanguageBranchID, ContentLinkGUID, fkFrameID, CreatorName, ChangedByName, ContentGUID, Name, URLSegment, LinkURL, ExternalURL, AutomaticLink, FetchData, Created, Changed, Saved, StartPublish, StopPublish, [Version], Status) VALUES (
		@fkContentId,--fkContentID
		@LanguageBranchID,--fkLanguageBranchID
		NULL,--ContentLinkGUID
		NULL,--fkFrameID
		'',--CreatorName
		'',--ChangedByName
		@campaignRootGuid,--ContentGUID
		@campaignRootName,--Name
		@campaignRootName,--URLSegment
		NULL,--LinkURL
		NULL,--ExternalURL
		1,--AutomaticLink
		0,--FetchData
		'19990101 00:00',--Created
		'19990101 00:00',--Changed
		'19990101 00:00',--Saved
		'19990101 00:00',--StartPublish
		NULL,--StopPublish
		@versionId,--Version
		4 --Status
		)
	END

IF EXISTS (SELECT 1 FROM tblContent INNER JOIN tblLanguageBranch ON fkMasterLanguageBranchID = tblLanguageBranch.pkID WHERE tblContent.pkID = @fkContentId AND LanguageID <> '') 
BEGIN
	UPDATE tblContent SET fkMasterLanguageBranchID = @LanguageBranchID
				WHERE pkID = @fkContentId

	UPDATE tblContentLanguage 
				SET fkLanguageBranchID = @LanguageBranchID
				FROM tblContentLanguage as lang INNER JOIN tblContent as cont on lang.fkContentId = cont.pkID
				WHERE cont.pkID = @fkContentId

	UPDATE tblContentCategory 
				SET fkLanguageBranchID = @LanguageBranchID
				FROM tblContentCategory as cat INNER JOIN tblContent as cont on cat.fkContentId = cont.pkID
				WHERE cont.pkID = @fkContentId

	UPDATE tblContentProperty 
				SET fkLanguageBranchID = @LanguageBranchID
				FROM tblContentProperty as prop INNER JOIN tblContent as cont on prop.fkContentId = cont.pkID
				WHERE cont.pkID = @fkContentId

	UPDATE tblWorkContent
				SET fkLanguageBranchID = @LanguageBranchID
				FROM tblWorkContent as workCont 
							 INNER JOIN tblContent as cont on workCont.fkContentID = cont.pkID
				WHERE cont.pkID = @fkContentId

	UPDATE tblContentSoftLink
				SET OwnerLanguageID = @LanguageBranchID
				FROM tblContentSoftLink as soft INNER JOIN tblContent as cont on soft.fkOwnerContentId = cont.pkID
				WHERE cont.pkID = @fkContentId

	UPDATE tblContentSoftLink
				SET ReferencedLanguageID = @LanguageBranchID
				FROM tblContentSoftLink as soft INNER JOIN tblContent as cont on soft.fkReferencedContentGUID = cont.ContentGUID
				WHERE cont.pkID = @fkContentId
END

GO