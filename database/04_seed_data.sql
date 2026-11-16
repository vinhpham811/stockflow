USE StockFlowDb;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    INSERT INTO dbo.Products (Sku, Name, Category, Price, Quantity, CreatedBy)
    VALUES
        (N'LAP-001', N'Laptop Dell Inspiron 15',      N'Laptop',    15990000, 12, N'seed'),
        (N'LAP-002', N'Laptop Lenovo ThinkPad E14',   N'Laptop',    18490000,  5, N'seed'),
        (N'LAP-003', N'MacBook Air M2',               N'Laptop',    24990000,  0, N'seed'),
        (N'MOU-001', N'Chuột Logitech M331',          N'Accessory',   390000, 50, N'seed'),
        (N'MOU-002', N'Chuột Razer DeathAdder',       N'Accessory',   990000,  8, N'seed'),
        (N'KEY-001', N'Bàn phím cơ Keychron K2',      N'Accessory',  1890000, 20, N'seed'),
        (N'MON-001', N'Màn hình LG 24 inch IPS',      N'Monitor',    3290000, 15, N'seed'),
        (N'MON-002', N'Màn hình Samsung 27 inch 2K',  N'Monitor',    5490000,  3, N'seed'),
        (N'HDP-001', N'Tai nghe Sony WH-1000XM5',     N'Audio',      7990000,  7, N'seed'),
        (N'SSD-001', N'SSD Samsung 980 1TB',          N'Storage',    2190000, 30, N'seed');
END
GO