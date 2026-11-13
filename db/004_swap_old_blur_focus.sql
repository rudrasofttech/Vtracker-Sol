SET NOCOUNT ON;

DECLARE @DeployDateUtc datetime2(0) = '2018-04-23'; -- REQUIRED: set before running
IF @DeployDateUtc IS NULL
    THROW 50040, '@DeployDateUtc must be set before running this script.', 1;

IF OBJECT_ID(N'dbo.ScriptExecutionLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ScriptExecutionLog
    (
        ScriptName nvarchar(200) NOT NULL PRIMARY KEY,
        ExecutedAtUtc datetime2(0) NOT NULL
    );
END

IF EXISTS (SELECT 1 FROM dbo.ScriptExecutionLog WHERE ScriptName = N'004_swap_old_blur_focus.sql')
BEGIN
    PRINT 'Script already executed once. Skipping.';
    RETURN;
END

BEGIN TRY
    BEGIN TRAN;

    DECLARE @BeforeBlur int = (
        SELECT COUNT(*) FROM dbo.VisitActivity WHERE Activity = 2 AND DateCreated < @DeployDateUtc
    );
    DECLARE @BeforeFocus int = (
        SELECT COUNT(*) FROM dbo.VisitActivity WHERE Activity = 3 AND DateCreated < @DeployDateUtc
    );

    UPDATE dbo.VisitActivity
    SET Activity = CASE
        WHEN Activity = 2 THEN 3
        WHEN Activity = 3 THEN 2
        ELSE Activity
    END
    WHERE DateCreated < @DeployDateUtc
      AND Activity IN (2, 3);

    DECLARE @Updated int = @@ROWCOUNT;

    INSERT INTO dbo.ScriptExecutionLog (ScriptName, ExecutedAtUtc)
    VALUES (N'004_swap_old_blur_focus.sql', SYSUTCDATETIME());

    COMMIT;

    PRINT CONCAT('Rows updated: ', @Updated);
    PRINT CONCAT('Before swap - Blur(2): ', @BeforeBlur, ', Focus(3): ', @BeforeFocus);
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH