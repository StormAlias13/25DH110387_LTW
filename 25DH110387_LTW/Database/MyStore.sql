
USE MyStore;
GO

ALTER AUTHORIZATION ON DATABASE::MyStore TO sa;
GO

-- Bang User: quan ly tai khoan cua Khach hang va Admin
IF OBJECT_ID('dbo.[User]', 'U') IS NULL
CREATE TABLE [dbo].[User] (
    [Username]  NVARCHAR (255) NOT NULL,
    [Password]  NCHAR (50)     NOT NULL,
    [UserRole]  NCHAR (1)      NOT NULL, -- A: admin, C: customer
    PRIMARY KEY CLUSTERED ([Username] ASC)
);
GO

-- Bang Customer: luu thong tin khach hang
IF OBJECT_ID('dbo.Customer', 'U') IS NULL
CREATE TABLE [dbo].[Customer] (
    [CustomerID]       INT IDENTITY (1, 1) NOT NULL,
    [CustomerName]     NVARCHAR (MAX) NOT NULL,
    [CustomerPhone]    NVARCHAR (15)  NOT NULL,
    [CustomerEmail]    NVARCHAR (MAX) NULL,
    [CustomerAddress]  NVARCHAR (MAX) NULL,
    [Username]         NVARCHAR (255) NOT NULL,
    PRIMARY KEY CLUSTERED ([CustomerID] ASC),
    CONSTRAINT [FK_User_Customer] FOREIGN KEY ([Username]) REFERENCES [dbo].[User] ([Username])
);
GO

-- Bang Category: luu thong tin danh muc san pham
IF OBJECT_ID('dbo.Category', 'U') IS NULL
CREATE TABLE [dbo].[Category] (
    [CategoryID]    INT IDENTITY (1, 1) NOT NULL,
    [CategoryName]  NVARCHAR (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([CategoryID] ASC)
);
GO

-- Bang Product: luu thong tin san pham
IF OBJECT_ID('dbo.Product', 'U') IS NULL
CREATE TABLE [dbo].[Product] (
    [ProductID]           INT IDENTITY (1, 1) NOT NULL,
    [CategoryID]          INT             NOT NULL,
    [ProductName]         NVARCHAR (MAX)  NOT NULL,
    [ProductDescription]  NVARCHAR (MAX)  NOT NULL,
    [ProductPrice]        DECIMAL (18, 2) NOT NULL,
    [ProductImage]        NVARCHAR (MAX)  NULL,
    PRIMARY KEY CLUSTERED ([ProductID] ASC),
    CONSTRAINT [FK_Pro_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Category] ([CategoryID])
);
GO

-- Bang Order: luu thong tin don hang cua khach hang
IF OBJECT_ID('dbo.[Order]', 'U') IS NULL
CREATE TABLE [dbo].[Order] (
    [OrderID]          INT IDENTITY (1, 1) NOT NULL,
    [CustomerID]       INT             NOT NULL,
    [OrderDate]        DATE            NOT NULL,
    [TotalAmount]      DECIMAL (18, 2) NOT NULL,
    [PaymentStatus]    NVARCHAR (MAX)  NULL,
    [AddressDelivery]  NVARCHAR (MAX)  NOT NULL,
    PRIMARY KEY CLUSTERED ([OrderID] ASC),
    FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer] ([CustomerID])
);
GO

-- Bang OrderDetail: luu chi tiet cac san pham trong don hang
IF OBJECT_ID('dbo.OrderDetail', 'U') IS NULL
CREATE TABLE [dbo].[OrderDetail] (
    [ID]         INT IDENTITY (1, 1) NOT NULL,
    [ProductID]  INT             NOT NULL,
    [OrderID]    INT             NOT NULL,
    [Quantity]   INT             NOT NULL,
    [UnitPrice]  DECIMAL (18, 2) NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Product] ([ProductID]),
    FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Order] ([OrderID])
);
GO

-- ===== Cap nhat cau truc =====

-- Order: them OrderStatus (phai co GO truoc khi UPDATE)
IF COL_LENGTH('dbo.[Order]', 'OrderStatus') IS NULL
    ALTER TABLE dbo.[Order] ADD OrderStatus NVARCHAR(50) NULL;
GO
UPDATE dbo.[Order] SET OrderStatus = N'Chờ xử lý' WHERE OrderStatus IS NULL;
GO

-- Order: doi AddressDelivery -> ShippingAddress
IF COL_LENGTH('dbo.[Order]', 'AddressDelivery') IS NOT NULL AND COL_LENGTH('dbo.[Order]', 'ShippingAddress') IS NULL
    EXEC sp_rename 'dbo.[Order].AddressDelivery', 'ShippingAddress', 'COLUMN';
GO

-- Order: them DeliveryMethod, PaymentMethod
IF COL_LENGTH('dbo.[Order]', 'DeliveryMethod') IS NULL
    ALTER TABLE dbo.[Order] ADD DeliveryMethod NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.[Order]', 'PaymentMethod') IS NULL
    ALTER TABLE dbo.[Order] ADD PaymentMethod NVARCHAR(MAX) NULL;
GO

-- OrderDetail: them TotalPrice tu tinh
IF COL_LENGTH('dbo.OrderDetail', 'TotalPrice') IS NULL
    ALTER TABLE dbo.OrderDetail ADD TotalPrice AS (Quantity * UnitPrice) PERSISTED;
GO

-- Product: them so luong ton kho
IF COL_LENGTH('dbo.Product', 'StockQuantity') IS NULL
    ALTER TABLE dbo.Product ADD StockQuantity INT NOT NULL CONSTRAINT DF_Product_StockQuantity DEFAULT (100);
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Product_StockQuantity')
    ALTER TABLE dbo.Product ADD CONSTRAINT CK_Product_StockQuantity CHECK (StockQuantity >= 0);
GO

-- Kiem tra ket qua
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Product';
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Order';
SELECT * FROM dbo.[Order];
GO
