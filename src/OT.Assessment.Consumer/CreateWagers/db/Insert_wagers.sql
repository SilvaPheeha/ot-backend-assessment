CREATE OR ALTER PROCEDURE dbo.InsertWagers
    @WagerId             UNIQUEIDENTIFIER,
    @Theme               NVARCHAR(100),
    @Provider            NVARCHAR(200),
    @GameName            NVARCHAR(200),
    @TransactionId       UNIQUEIDENTIFIER,
    @BrandId             UNIQUEIDENTIFIER,
    @AccountId           UNIQUEIDENTIFIER,
    @Username            NVARCHAR(100),
    @ExternalReferenceId UNIQUEIDENTIFIER,
    @TransactionTypeId   UNIQUEIDENTIFIER,
    @Amount              DECIMAL(18, 6),
    @CreatedDateTime     DATETIMEOFFSET(7),
    @NumberOfBets        INT,
    @CountryCode         CHAR(10),
    @SessionData         NVARCHAR(MAX) = NULL, 
    @Duration            BIGINT = NULL
AS
-- Returns a DbState value: 1 = INSERTED, 5 = DUPLICATE.
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @BetId UNIQUEIDENTIFIER = NEWID();
    DECLARE @UserId UNIQUEIDENTIFIER;

    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM dbo.Wagers WITH (UPDLOCK, HOLDLOCK) WHERE WagerId = @WagerId)
    BEGIN
        COMMIT TRANSACTION;
        RETURN 5;
    END

  -- SHOULD RETURN ERROR : BRAND NOT FOUND, BUT FOR NOW, CREATE A NEW BRAND IF NOT FOUND. 
    IF NOT EXISTS (SELECT 1 FROM dbo.Brands WITH (UPDLOCK, HOLDLOCK) WHERE BrandId = @BrandId)
    BEGIN
        INSERT INTO dbo.Brands (BrandId, BrandName)
        VALUES (@BrandId, N'Unknown Brand');
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.TransactionType WITH (UPDLOCK, HOLDLOCK) WHERE TransactionTypeId = @TransactionTypeId)
    BEGIN
        INSERT INTO dbo.TransactionType (TransactionTypeId, TransactionType, TypeName)
        VALUES (@TransactionTypeId, N'Unknown', N'Unknown');
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Accounts WITH (UPDLOCK, HOLDLOCK) WHERE AccountId = @AccountId)
    BEGIN
        SELECT TOP (1) @UserId = UserId
        FROM dbo.Users WITH (UPDLOCK, HOLDLOCK)
        WHERE Username = @Username;

        -- SHOULD RETURN ERROR : USER NOT FOUND, BUT FOR NOW, CREATE A NEW USER IF NOT FOUND. 
        IF @UserId IS NULL
        BEGIN
            SET @UserId = NEWID();
            
            INSERT INTO dbo.Users (UserId, Username, Name, Surname, Email, PhoneNumber, CountryCode)
            VALUES (@UserId, @Username, @Username, N'', N'', N'', @CountryCode);
        END

        INSERT INTO dbo.Accounts (AccountId, UserId)
        VALUES (@AccountId, @UserId);
    END

    INSERT INTO dbo.Bets (BetId, NoOfBets, Amount, CreatedDateTime)
    VALUES (@BetId, @NumberOfBets, @Amount, @CreatedDateTime);

    INSERT INTO dbo.Transactions (TransactionId, AccountId, TransactionTypeId, Amount, TransactionDate, CreatedDateTime)
    VALUES (@TransactionId, @AccountId, @TransactionTypeId, @Amount, @CreatedDateTime, @CreatedDateTime);

    INSERT INTO dbo.Wagers
    (
        WagerId, Theme, Provider, GameName, CountryCode, TransactionId, BrandId, AccountId,
        ExternalReferenceId, TransactionTypeId, BetId, SessionData, Duration, IsActive, CreatedDateTime
    )
    VALUES
    (
        @WagerId, @Theme, @Provider, @GameName, @CountryCode, @TransactionId, @BrandId, @AccountId,
        @ExternalReferenceId, @TransactionTypeId, @BetId, @SessionData, @Duration, 1, @CreatedDateTime
    );

    COMMIT TRANSACTION;
    RETURN 1;
END
