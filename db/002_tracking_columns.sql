SET NOCOUNT ON;

PRINT 'Phase 4.1 tracking columns script starting...';

DECLARE @VisitWebsiteCol sysname;
SELECT TOP (1) @VisitWebsiteCol = c.name
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.Visit')
  AND c.name IN (N'WebsiteID', N'websiteID', N'Website_ID')
ORDER BY CASE c.name WHEN N'WebsiteID' THEN 1 WHEN N'websiteID' THEN 2 ELSE 3 END;

IF @VisitWebsiteCol IS NULL
    THROW 50010, 'Could not resolve Website FK column on dbo.Visit.', 1;

/* Visit */
IF COL_LENGTH('dbo.Visit', 'VisitorId') IS NULL
BEGIN
    ALTER TABLE dbo.Visit ADD VisitorId uniqueidentifier NULL;
END

IF COL_LENGTH('dbo.Visit', 'UserAgent') IS NULL
BEGIN
    ALTER TABLE dbo.Visit ADD UserAgent nvarchar(400) NULL;
END

IF COL_LENGTH('dbo.Visit', 'IsBot') IS NULL
BEGIN
    ALTER TABLE dbo.Visit ADD IsBot bit NOT NULL CONSTRAINT DF_Visit_IsBot DEFAULT (0) WITH VALUES;
END

IF COL_LENGTH('dbo.Visit', 'UtmSource') IS NULL
BEGIN
    ALTER TABLE dbo.Visit ADD UtmSource nvarchar(100) NULL;
END

IF COL_LENGTH('dbo.Visit', 'UtmMedium') IS NULL
BEGIN
    ALTER TABLE dbo.Visit ADD UtmMedium nvarchar(100) NULL;
END

IF COL_LENGTH('dbo.Visit', 'UtmCampaign') IS NULL
BEGIN
    ALTER TABLE dbo.Visit ADD UtmCampaign nvarchar(100) NULL;
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Visit')
      AND name = N'IX_Visit_WebsiteID_VisitorId'
)
BEGIN
    DECLARE @sqlVisitVisitorIndex nvarchar(max) =
        N'CREATE INDEX IX_Visit_WebsiteID_VisitorId
          ON dbo.Visit (' + QUOTENAME(@VisitWebsiteCol) + N', VisitorId)
          WHERE VisitorId IS NOT NULL;';
    EXEC sp_executesql @sqlVisitVisitorIndex;
END

/* VisitPage */
IF COL_LENGTH('dbo.VisitPage', 'MaxScrollPct') IS NULL
BEGIN
    ALTER TABLE dbo.VisitPage ADD MaxScrollPct tinyint NULL;
END

IF COL_LENGTH('dbo.VisitPage', 'DocumentHeight') IS NULL
BEGIN
    ALTER TABLE dbo.VisitPage ADD DocumentHeight int NULL;
END

/* VisitActivity */
IF COL_LENGTH('dbo.VisitActivity', 'ClickHref') IS NULL
BEGIN
    ALTER TABLE dbo.VisitActivity ADD ClickHref nvarchar(500) NULL;
END

IF COL_LENGTH('dbo.VisitActivity', 'ClickText') IS NULL
BEGIN
    ALTER TABLE dbo.VisitActivity ADD ClickText nvarchar(100) NULL;
END

/* Website */
IF COL_LENGTH('dbo.Website', 'TimeZone') IS NULL
BEGIN
    ALTER TABLE dbo.Website ADD TimeZone varchar(64) NOT NULL
        CONSTRAINT DF_Website_TimeZone DEFAULT ('India Standard Time') WITH VALUES;
END

PRINT 'Phase 4.1 tracking columns script completed.';