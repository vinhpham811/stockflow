USE StockFlowDb;
GO

-- 1. Liệt kê các bảng (phải thấy 6 bảng)
SELECT name FROM sys.tables ORDER BY name;

-- 2. Liệt kê các SP (phải thấy 3 SP)
SELECT name FROM sys.procedures ORDER BY name;

-- 3. Test tìm kiếm: phải ra 2 dòng chuột, TotalCount = 2
EXEC dbo.usp_Product_SearchPaged @Keyword = N'Chuột';

-- 4. Test phân trang: trang 2, mỗi trang 3 dòng
EXEC dbo.usp_Product_SearchPaged @PageNumber = 2, @PageSize = 3;

-- 5. Test báo cáo: LAP-003 = Out of stock, MON-002 = Low stock
EXEC dbo.usp_Report_Inventory;

-- 6. Test merge import (chạy trong transaction rồi ROLLBACK để không làm bẩn dữ liệu)
BEGIN TRANSACTION;

    INSERT INTO dbo.ImportJobs (FileName, BlobPath, RequestedBy)
    VALUES (N'test.xlsx', N'imports/test.xlsx', N'tester');

    DECLARE @JobId INT = SCOPE_IDENTITY();

    INSERT INTO dbo.ImportProductStaging (ImportJobId, Sku, Name, Category, Price, Quantity)
    VALUES
        (@JobId, N'LAP-001', N'Laptop Dell Inspiron 15 (new)', N'Laptop', 14990000, 99),  -- update
        (@JobId, N'USB-001', N'USB Kingston 64GB',             N'Storage',  150000, 40),  -- insert
        (@JobId, N'USB-001', N'USB Kingston 64GB v2',          N'Storage',  160000, 45);  -- trùng SKU, lấy dòng này

    EXEC dbo.usp_Import_MergeProducts @ImportJobId = @JobId;   -- phải ra SuccessRows = 2

    SELECT Sku, Name, Price, Quantity FROM dbo.Products WHERE Sku IN (N'LAP-001', N'USB-001');

ROLLBACK TRANSACTION;