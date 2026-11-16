USE StockFlowDb;
GO

/* =========================================================
   SP 1: Tìm kiếm sản phẩm có phân trang
   Trả về TotalCount trên mỗi dòng để FE biết tổng số trang
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.usp_Product_SearchPaged
    @Keyword     NVARCHAR(200) = NULL,
    @Category    NVARCHAR(100) = NULL,
    @PageNumber  INT = 1,
    @PageSize    INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 OR @PageSize > 100 SET @PageSize = 10;

    SELECT
        Id,
        Sku,
        Name,
        Category,
        Price,
        Quantity,
        CreatedAt,
        UpdatedAt,
        COUNT(*) OVER () AS TotalCount
    FROM dbo.Products
    WHERE IsDeleted = 0
      AND (@Keyword IS NULL OR Name LIKE N'%' + @Keyword + N'%' OR Sku LIKE N'%' + @Keyword + N'%')
      AND (@Category IS NULL OR Category = @Category)
    ORDER BY Id DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

/* =========================================================
   SP 2: Merge dữ liệu import từ bảng staging vào Products
   - SKU đã có  -> cập nhật
   - SKU chưa có -> thêm mới
   - SKU trùng trong cùng 1 file -> lấy dòng cuối cùng
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.usp_Import_MergeProducts
    @ImportJobId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.ImportJobs WHERE Id = @ImportJobId)
    BEGIN
        THROW 50001, N'ImportJob does not exist.', 1;
    END

    DECLARE @RequestedBy NVARCHAR(256);
    DECLARE @SuccessRows INT = 0;

    SELECT @RequestedBy = RequestedBy
    FROM dbo.ImportJobs
    WHERE Id = @ImportJobId;

    BEGIN TRY
        BEGIN TRANSACTION;

        WITH LatestRows AS
        (
            SELECT
                Sku, Name, Category, Price, Quantity,
                ROW_NUMBER() OVER (PARTITION BY Sku ORDER BY Id DESC) AS RowNum
            FROM dbo.ImportProductStaging
            WHERE ImportJobId = @ImportJobId
        )
        MERGE dbo.Products AS target
        USING (SELECT Sku, Name, Category, Price, Quantity FROM LatestRows WHERE RowNum = 1) AS source
            ON target.Sku = source.Sku
        WHEN MATCHED THEN
            UPDATE SET
                target.Name      = source.Name,
                target.Category  = source.Category,
                target.Price     = source.Price,
                target.Quantity  = source.Quantity,
                target.IsDeleted = 0,
                target.UpdatedAt = SYSUTCDATETIME(),
                target.UpdatedBy = @RequestedBy
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (Sku, Name, Category, Price, Quantity, CreatedBy)
            VALUES (source.Sku, source.Name, source.Category, source.Price, source.Quantity, @RequestedBy);

        SET @SuccessRows = @@ROWCOUNT;

        UPDATE dbo.ImportJobs
        SET SuccessRows = @SuccessRows
        WHERE Id = @ImportJobId;

        DELETE FROM dbo.ImportProductStaging
        WHERE ImportJobId = @ImportJobId;

        COMMIT TRANSACTION;

        SELECT @SuccessRows AS SuccessRows;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO

/* =========================================================
   SP 3: Báo cáo tồn kho (dùng cho chức năng xuất Excel)
   ========================================================= */
CREATE OR ALTER PROCEDURE dbo.usp_Report_Inventory
    @Category           NVARCHAR(100) = NULL,
    @LowStockThreshold  INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Sku,
        Name,
        ISNULL(Category, N'(Uncategorized)') AS Category,
        Price,
        Quantity,
        Price * Quantity AS StockValue,
        CASE
            WHEN Quantity = 0                   THEN N'Out of stock'
            WHEN Quantity < @LowStockThreshold  THEN N'Low stock'
            ELSE N'In stock'
        END AS StockStatus
    FROM dbo.Products
    WHERE IsDeleted = 0
      AND (@Category IS NULL OR Category = @Category)
    ORDER BY Category, Name;
END
GO