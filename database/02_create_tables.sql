USE StockFlowDb;
GO

/* =========================================================
   1. Products - Sản phẩm (bảng chính)
   ========================================================= */
IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id          INT IDENTITY(1,1) NOT NULL,
        Sku         NVARCHAR(50)      NOT NULL,
        Name        NVARCHAR(200)     NOT NULL,
        Category    NVARCHAR(100)     NULL,
        Price       DECIMAL(18,2)     NOT NULL,
        Quantity    INT               NOT NULL CONSTRAINT DF_Products_Quantity  DEFAULT (0),
        IsDeleted   BIT               NOT NULL CONSTRAINT DF_Products_IsDeleted DEFAULT (0),
        CreatedAt   DATETIME2         NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy   NVARCHAR(256)     NOT NULL,
        UpdatedAt   DATETIME2         NULL,
        UpdatedBy   NVARCHAR(256)     NULL,

        CONSTRAINT PK_Products           PRIMARY KEY (Id),
        CONSTRAINT UQ_Products_Sku       UNIQUE (Sku),
        CONSTRAINT CK_Products_Price     CHECK (Price >= 0),
        CONSTRAINT CK_Products_Quantity  CHECK (Quantity >= 0)
    );

    CREATE INDEX IX_Products_Category ON dbo.Products (Category);
END
GO

/* =========================================================
   2. ProductHistories - Lịch sử thay đổi giá/tồn kho (Kafka ghi vào)
   ========================================================= */
IF OBJECT_ID(N'dbo.ProductHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductHistories
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL,
        ProductId   INT                  NOT NULL,
        FieldName   NVARCHAR(50)         NOT NULL,   -- 'Price' hoặc 'Quantity'
        OldValue    NVARCHAR(100)        NULL,
        NewValue    NVARCHAR(100)        NULL,
        ChangedBy   NVARCHAR(256)        NOT NULL,
        ChangedAt   DATETIME2            NOT NULL CONSTRAINT DF_ProductHistories_ChangedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_ProductHistories PRIMARY KEY (Id),
        CONSTRAINT FK_ProductHistories_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products (Id)
    );

    CREATE INDEX IX_ProductHistories_ProductId ON dbo.ProductHistories (ProductId);
END
GO

/* =========================================================
   3. Notifications - Thông báo/email giả lập (RabbitMQ ghi vào)
   ========================================================= */
IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL,
        Recipient   NVARCHAR(256)        NOT NULL,
        Subject     NVARCHAR(200)        NOT NULL,
        Body        NVARCHAR(MAX)        NOT NULL,
        Status      NVARCHAR(20)         NOT NULL CONSTRAINT DF_Notifications_Status DEFAULT (N'Sent'),
        CreatedAt   DATETIME2            NOT NULL CONSTRAINT DF_Notifications_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Notifications PRIMARY KEY (Id),
        CONSTRAINT CK_Notifications_Status CHECK (Status IN (N'Sent', N'Failed'))
    );
END
GO

/* =========================================================
   4. ImportJobs - Job import Excel (BackgroundService xử lý)
   ========================================================= */
IF OBJECT_ID(N'dbo.ImportJobs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ImportJobs
    (
        Id            INT IDENTITY(1,1) NOT NULL,
        FileName      NVARCHAR(260)     NOT NULL,
        BlobPath      NVARCHAR(500)     NOT NULL,
        Status        NVARCHAR(20)      NOT NULL CONSTRAINT DF_ImportJobs_Status DEFAULT (N'Pending'),
        TotalRows     INT               NOT NULL CONSTRAINT DF_ImportJobs_TotalRows   DEFAULT (0),
        SuccessRows   INT               NOT NULL CONSTRAINT DF_ImportJobs_SuccessRows DEFAULT (0),
        ErrorMessage  NVARCHAR(MAX)     NULL,
        RequestedBy   NVARCHAR(256)     NOT NULL,
        CreatedAt     DATETIME2         NOT NULL CONSTRAINT DF_ImportJobs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CompletedAt   DATETIME2         NULL,

        CONSTRAINT PK_ImportJobs PRIMARY KEY (Id),
        CONSTRAINT CK_ImportJobs_Status CHECK (Status IN (N'Pending', N'Processing', N'Completed', N'Failed'))
    );

    CREATE INDEX IX_ImportJobs_Status ON dbo.ImportJobs (Status);
END
GO

/* =========================================================
   5. ImportProductStaging - Bảng tạm chứa dòng đọc từ Excel
   ========================================================= */
IF OBJECT_ID(N'dbo.ImportProductStaging', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ImportProductStaging
    (
        Id           BIGINT IDENTITY(1,1) NOT NULL,
        ImportJobId  INT                  NOT NULL,
        Sku          NVARCHAR(50)         NOT NULL,
        Name         NVARCHAR(200)        NOT NULL,
        Category     NVARCHAR(100)        NULL,
        Price        DECIMAL(18,2)        NOT NULL,
        Quantity     INT                  NOT NULL,

        CONSTRAINT PK_ImportProductStaging PRIMARY KEY (Id),
        CONSTRAINT FK_ImportProductStaging_ImportJobs FOREIGN KEY (ImportJobId) REFERENCES dbo.ImportJobs (Id)
    );

    CREATE INDEX IX_ImportProductStaging_ImportJobId ON dbo.ImportProductStaging (ImportJobId);
END
GO

/* =========================================================
   6. ReportJobs - Job xuất báo cáo (Azure Service Bus xử lý)
   ========================================================= */
IF OBJECT_ID(N'dbo.ReportJobs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReportJobs
    (
        Id            INT IDENTITY(1,1) NOT NULL,
        ReportType    NVARCHAR(50)      NOT NULL,   -- 'Inventory'
        Status        NVARCHAR(20)      NOT NULL CONSTRAINT DF_ReportJobs_Status DEFAULT (N'Pending'),
        FilePath      NVARCHAR(500)     NULL,
        ErrorMessage  NVARCHAR(MAX)     NULL,
        RequestedBy   NVARCHAR(256)     NOT NULL,
        CreatedAt     DATETIME2         NOT NULL CONSTRAINT DF_ReportJobs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CompletedAt   DATETIME2         NULL,

        CONSTRAINT PK_ReportJobs PRIMARY KEY (Id),
        CONSTRAINT CK_ReportJobs_Status CHECK (Status IN (N'Pending', N'Processing', N'Completed', N'Failed'))
    );
END
GO