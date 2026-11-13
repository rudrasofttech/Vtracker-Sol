SET NOCOUNT ON;

PRINT 'Phase 4.5 backfill IsBot script starting...';

IF COL_LENGTH('dbo.Visit', 'IsBot') IS NULL
BEGIN
    PRINT 'dbo.Visit.IsBot does not exist. Run 002_tracking_columns.sql first.';
    RETURN;
END

UPDATE dbo.Visit
SET IsBot = 1
WHERE IsBot = 0
  AND (
      ISNULL(IsProxy, 0) = 1
      OR (ScreenWidth = 1600 AND ScreenHeight = 1600)
      OR (ScreenWidth = 800 AND ScreenHeight = 600)
  );

PRINT CONCAT('Rows flagged as bot: ', @@ROWCOUNT);
PRINT 'Phase 4.5 backfill IsBot script completed.';