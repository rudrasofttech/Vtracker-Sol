SET NOCOUNT ON;

PRINT 'Phase 1.1 + 1.2 index script starting...';

DECLARE @VisitTable sysname = N'dbo.Visit';
DECLARE @VisitPageTable sysname = N'dbo.VisitPage';
DECLARE @VisitActivityTable sysname = N'dbo.VisitActivity';
DECLARE @WebpageTable sysname = N'dbo.Webpage';

DECLARE @VisitWebsiteCol sysname;
DECLARE @VisitPageVisitCol sysname;
DECLARE @VisitPageWebpageCol sysname;
DECLARE @VisitActivityVisitCol sysname;
DECLARE @VisitActivityVisitPageCol sysname;
DECLARE @WebpageWebsiteCol sysname;

SELECT TOP (1) @VisitWebsiteCol = c.name
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(@VisitTable)
  AND c.name IN (N'WebsiteID', N'websiteID', N'Website_ID')
ORDER BY CASE c.name WHEN N'WebsiteID' THEN 1 WHEN N'websiteID' THEN 2 ELSE 3 END;

SELECT TOP (1) @VisitPageVisitCol = c.name
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(@VisitPageTable)
  AND c.name IN (N'VisitID', N'visitID', N'visit_ID')
ORDER BY CASE c.name WHEN N'VisitID' THEN 1 WHEN N'visitID' THEN 2 ELSE 3 END;

SELECT TOP (1) @VisitPageWebpageCol = c.name
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(@VisitPageTable)
  AND c.name IN (N'WebpageID', N'webpageID', N'webpage_ID')
ORDER BY CASE c.name WHEN N'WebpageID' THEN 1 WHEN N'webpageID' THEN 2 ELSE 3 END;

SELECT TOP (1) @VisitActivityVisitCol = c.name
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(@VisitActivityTable)
  AND c.name IN (N'VisitID', N'visitID', N'visit_ID')
ORDER BY CASE c.name WHEN N'VisitID' THEN 1 WHEN N'visitID' THEN 2 ELSE 3 END;

SELECT TOP (1) @VisitActivityVisitPageCol = c.name
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(@VisitActivityTable)
  AND c.name IN (N'VisitPageID', N'visitpageID', N'visitpage_ID')
ORDER BY CASE c.name WHEN N'VisitPageID' THEN 1 WHEN N'visitpageID' THEN 2 ELSE 3 END;

SELECT TOP (1) @WebpageWebsiteCol = c.name
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(@WebpageTable)
  AND c.name IN (N'WebsiteID', N'websiteID', N'Website_ID')
ORDER BY CASE c.name WHEN N'WebsiteID' THEN 1 WHEN N'websiteID' THEN 2 ELSE 3 END;

IF @VisitWebsiteCol IS NULL
    THROW 50001, 'Could not resolve Website FK column in dbo.Visit.', 1;
IF @VisitPageVisitCol IS NULL OR @VisitPageWebpageCol IS NULL
    THROW 50002, 'Could not resolve VisitPage FK columns in dbo.VisitPage.', 1;
IF @VisitActivityVisitCol IS NULL OR @VisitActivityVisitPageCol IS NULL
    THROW 50003, 'Could not resolve VisitActivity FK columns in dbo.VisitActivity.', 1;
IF @WebpageWebsiteCol IS NULL
    THROW 50004, 'Could not resolve Website FK column in dbo.Webpage.', 1;

/* 1.2 PathHash computed column for fast webpage lookup by (Website + normalized Path+Query hash) */
IF COL_LENGTH('dbo.Webpage', 'PathHash') IS NULL
BEGIN
    ALTER TABLE dbo.Webpage
    ADD PathHash AS CAST(HASHBYTES('SHA2_256', LOWER([Path]) + '?' + LOWER([QueryString])) AS binary(32)) PERSISTED;
    PRINT 'Added dbo.Webpage.PathHash persisted computed column.';
END
ELSE
BEGIN
    PRINT 'dbo.Webpage.PathHash already exists.';
END

/* 1.1 Visit(ClientCookie): serves GetVisitByCC and all pulse/click/focus/blur lookups */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Visit') AND name = 'IX_Visit_ClientCookie')
BEGIN
    IF EXISTS (
        SELECT 1
        FROM dbo.Visit
        GROUP BY ClientCookie
        HAVING COUNT(*) > 1
    )
    BEGIN
        PRINT 'Duplicate ClientCookie values found. Creating NON-UNIQUE IX_Visit_ClientCookie.';
        SELECT ClientCookie, COUNT(*) AS DuplicateCount
        FROM dbo.Visit
        GROUP BY ClientCookie
        HAVING COUNT(*) > 1
        ORDER BY DuplicateCount DESC;

        CREATE INDEX IX_Visit_ClientCookie
            ON dbo.Visit (ClientCookie);
    END
    ELSE
    BEGIN
        PRINT 'No duplicate ClientCookie values found. Creating UNIQUE IX_Visit_ClientCookie.';
        CREATE UNIQUE INDEX IX_Visit_ClientCookie
            ON dbo.Visit (ClientCookie);
    END
END
ELSE
BEGIN
    PRINT 'IX_Visit_ClientCookie already exists.';
END

/* 1.1 Visit(WebsiteID, DateCreated) INCLUDE(...): serves website/date-range report queries */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Visit') AND name = 'IX_Visit_WebsiteID_DateCreated')
BEGIN
    DECLARE @sqlVisitWebsiteDate nvarchar(max) =
        N'CREATE INDEX IX_Visit_WebsiteID_DateCreated
          ON dbo.Visit (' + QUOTENAME(@VisitWebsiteCol) + N', DateCreated)
          INCLUDE (LastVisitID, BrowserName, Referer, ScreenWidth, ScreenHeight);';
    EXEC sp_executesql @sqlVisitWebsiteDate;
END
ELSE
BEGIN
    PRINT 'IX_Visit_WebsiteID_DateCreated already exists.';
END

/* 1.1 VisitPage(VisitID, ID DESC) INCLUDE(WebpageID): serves "last page of visit" lookups */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.VisitPage') AND name = 'IX_VisitPage_VisitID_ID_DESC')
BEGIN
    DECLARE @sqlVisitPageLast nvarchar(max) =
        N'CREATE INDEX IX_VisitPage_VisitID_ID_DESC
          ON dbo.VisitPage (' + QUOTENAME(@VisitPageVisitCol) + N', ID DESC)
          INCLUDE (' + QUOTENAME(@VisitPageWebpageCol) + N');';
    EXEC sp_executesql @sqlVisitPageLast;
END
ELSE
BEGIN
    PRINT 'IX_VisitPage_VisitID_ID_DESC already exists.';
END

/* 1.1 VisitPage(WebpageID, DateCreated): serves page-level time-range report queries */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.VisitPage') AND name = 'IX_VisitPage_WebpageID_DateCreated')
BEGIN
    DECLARE @sqlVisitPageWebpageDate nvarchar(max) =
        N'CREATE INDEX IX_VisitPage_WebpageID_DateCreated
          ON dbo.VisitPage (' + QUOTENAME(@VisitPageWebpageCol) + N', DateCreated);';
    EXEC sp_executesql @sqlVisitPageWebpageDate;
END
ELSE
BEGIN
    PRINT 'IX_VisitPage_WebpageID_DateCreated already exists.';
END

/* 1.1 VisitActivity(VisitID): serves visit-activity queries by visit */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.VisitActivity') AND name = 'IX_VisitActivity_VisitID')
BEGIN
    DECLARE @sqlVisitActivityVisit nvarchar(max) =
        N'CREATE INDEX IX_VisitActivity_VisitID
          ON dbo.VisitActivity (' + QUOTENAME(@VisitActivityVisitCol) + N');';
    EXEC sp_executesql @sqlVisitActivityVisit;
END
ELSE
BEGIN
    PRINT 'IX_VisitActivity_VisitID already exists.';
END

/* 1.1 VisitActivity(VisitPageID) INCLUDE(...): serves click/blur/focus page activity reads */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.VisitActivity') AND name = 'IX_VisitActivity_VisitPageID_Inc')
BEGIN
    DECLARE @sqlVisitActivityPage nvarchar(max) =
        N'CREATE INDEX IX_VisitActivity_VisitPageID_Inc
          ON dbo.VisitActivity (' + QUOTENAME(@VisitActivityVisitPageCol) + N')
          INCLUDE (Activity, MouseClickX, MouseClickY, ClickTagName, ClickTagId);';
    EXEC sp_executesql @sqlVisitActivityPage;
END
ELSE
BEGIN
    PRINT 'IX_VisitActivity_VisitPageID_Inc already exists.';
END

/* 1.1 Webpage(WebsiteID) INCLUDE(Path, QueryString): serves website-page lookup/report expansion */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Webpage') AND name = 'IX_Webpage_WebsiteID_Inc_PathQuery')
BEGIN
    DECLARE @sqlWebpageWebsite nvarchar(max) =
        N'CREATE INDEX IX_Webpage_WebsiteID_Inc_PathQuery
          ON dbo.Webpage (' + QUOTENAME(@WebpageWebsiteCol) + N')
          INCLUDE (Path, QueryString);';
    EXEC sp_executesql @sqlWebpageWebsite;
END
ELSE
BEGIN
    PRINT 'IX_Webpage_WebsiteID_Inc_PathQuery already exists.';
END

/* 1.2 Webpage(WebsiteID, PathHash): seekable lookup for normalized path+query */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.Webpage') AND name = 'IX_Webpage_WebsiteID_PathHash')
BEGIN
    DECLARE @sqlWebpagePathHash nvarchar(max) =
        N'CREATE INDEX IX_Webpage_WebsiteID_PathHash
          ON dbo.Webpage (' + QUOTENAME(@WebpageWebsiteCol) + N', PathHash);';
    EXEC sp_executesql @sqlWebpagePathHash;
END
ELSE
BEGIN
    PRINT 'IX_Webpage_WebsiteID_PathHash already exists.';
END

PRINT 'Phase 1.1 + 1.2 index script completed.';