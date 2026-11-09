IF DB_ID(N'$(DB_NAME)') IS NULL
BEGIN
    CREATE DATABASE [$(DB_NAME)];
END
GO

USE [$(DB_NAME)];
GO
IF OBJECT_ID(N'dbo.TransactionType', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TransactionType
    (
        TransactionTypeId   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TransactionType     NVARCHAR(100) NOT NULL, 
        TypeName            NVARCHAR(100) NOT NULL, 
        IsActive            BIT  NOT NULL DEFAULT 1,
        CreatedDateTime     DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        UpdatedDateTime     DATETIMEOFFSET(7)  NULL,
    )
END

IF OBJECT_ID(N'dbo.Bets', N'U') IS NULL
BEGIN 
    CREATE TABLE dbo.Bets
    (
        BetId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        NoOfBets    INT  NOT NULL,
        Amount      DECIMAL(18, 6)NOT NULL,
        IsActive            BIT NOT NULL DEFAULT 1,
        CreatedDateTime     DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        UpdatedDateTime     DATETIMEOFFSET(7)  NULL,
    )
END

IF OBJECT_ID(N'dbo.Brands', N'U') IS NULL
BEGIN 
    CREATE TABLE dbo.Brands
    (
        BrandId             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        BrandName           NVARCHAR(100)  NOT NULL,
        IsActive            BIT  NOT NULL DEFAULT 1,
        CreatedDateTime     DATETIMEOFFSET(7)  NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        UpdatedDateTime     DATETIMEOFFSET(7)  NULL,
    )
END

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Username    NVARCHAR(100) NOT NULL,
        Name        NVARCHAR(100) NOT NULL,
        Surname     NVARCHAR(100) NOT NULL,
        Email       NVARCHAR(100) NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        IdNumber    NVARCHAR(200) NULL,
        IdType      NVARCHAR(200) NULL,
        CountryCode     CHAR(10) NULL,
        IsActive        BIT  NOT NULL DEFAULT 1,
        CreatedDateTime DATETIMEOFFSET(7)  NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        UpdatedDateTime DATETIMEOFFSET(7)  NULL,
    )
END

IF OBJECT_ID(N'dbo.Accounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Accounts
    (
        AccountId   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        UserId    UNIQUEIDENTIFIER NOT NULL,
        AccountName NVARCHAR(200) NULL,
        AccountNumber NVARCHAR(200) NULL,
        AccountType NVARCHAR(200) NULL,
        BranchCode NVARCHAR(200) NULL,
        IsActive        BIT  NOT NULL DEFAULT 1,
        CreatedDateTime DATETIMEOFFSET(7)  NOT NULL DEFAULT SYSDATETIMEOFFSET(),  
        UpdatedDateTime DATETIMEOFFSET(7)  NULL,
        CONSTRAINT fk_user_id FOREIGN KEY (UserId) REFERENCES Users(UserId)
    )
    
    CREATE INDEX IX_Accounts_AccountId_CreatedDateTime
        ON dbo.Accounts (AccountId, CreatedDateTime DESC);
END

IF OBJECT_ID(N'dbo.Transactions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Transactions
    (
        TransactionId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        AccountId           UNIQUEIDENTIFIER NOT NULL,
        TransactionTypeId   UNIQUEIDENTIFIER NOT NULL,
        Amount              DECIMAL(15, 2) NOT NULL,
        TransactionDate     DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        Description         VARCHAR(255) NULL,
        IsActive            BIT  NOT NULL DEFAULT 1,
        CreatedDateTime     DATETIMEOFFSET(7) NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        UpdatedDateTime     DATETIMEOFFSET(7)  NULL,
        CONSTRAINT fk_transactions_account_id FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
        CONSTRAINT fk_transactions_transaction_type_id FOREIGN KEY (TransactionTypeId) REFERENCES TransactionType(TransactionTypeId)
    )
END

IF OBJECT_ID(N'dbo.Wagers', N'U') IS NULL
BEGIN
        CREATE TABLE dbo.Wagers
        (
            WagerId             UNIQUEIDENTIFIER   NOT NULL PRIMARY KEY,
            Theme               NVARCHAR(100)      NOT NULL,
            Provider            NVARCHAR(200)      NOT NULL,
            GameName            NVARCHAR(200)      NOT NULL,
            CountryCode         CHAR(10) NOT NULL,
            TransactionId       UNIQUEIDENTIFIER   NOT NULL,
            BrandId             UNIQUEIDENTIFIER   NOT NULL,
            AccountId           UNIQUEIDENTIFIER   NOT NULL,
            ExternalReferenceId UNIQUEIDENTIFIER   NOT NULL,
            TransactionTypeId   UNIQUEIDENTIFIER   NOT NULL,
            BetId               UNIQUEIDENTIFIER   NOT NULL,
            SessionData         NVARCHAR(MAX)      NULL,
            Duration            BIGINT             NULL,
            IsActive            BIT                NOT NULL,
            CreatedDateTime     DATETIMEOFFSET(7)  NOT NULL DEFAULT SYSDATETIMEOFFSET(),
            UpdatedDateTime     DATETIMEOFFSET(7)  NULL,
            CONSTRAINT fk_transaction_id FOREIGN KEY (TransactionId) REFERENCES Transactions(TransactionId),
            CONSTRAINT fk_brand_id FOREIGN KEY (BrandId) REFERENCES Brands(BrandId),
            CONSTRAINT fk_account_id FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
            CONSTRAINT fk_transaction_type_id FOREIGN KEY (TransactionTypeId) REFERENCES TransactionType(TransactionTypeId),
            CONSTRAINT fk_bet_id FOREIGN KEY (BetId) REFERENCES Bets(BetId)
        );

        CREATE INDEX IX_Wagers_WagerId_CreatedDateTime
            ON dbo.Wagers (WagerId, CreatedDateTime DESC);
    END
GO

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
GO
