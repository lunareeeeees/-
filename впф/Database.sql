
USE [master];
GO
IF DB_ID(N'ShopSimple') IS NULL
    EXEC(N'CREATE DATABASE [ShopSimple]');
GO
USE [ShopSimple];
GO
IF OBJECT_ID(N'dbo.Товары', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[Товары]
    (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Название] NVARCHAR(100) NOT NULL,
        [Цена] DECIMAL(10,2) NOT NULL CHECK ([Цена] >= 0),
        [Количество] INT NOT NULL CHECK ([Количество] >= 0)
    );

    INSERT INTO dbo.[Товары] ([Название], [Цена], [Количество])
    VALUES (N'Молоко', 90.00, 20), (N'Хлеб', 45.00, 30), (N'Вода', 60.00, 40);
END;
GO
IF DATABASE_PRINCIPAL_ID(N'Administrator') IS NULL
    EXEC(N'CREATE ROLE [Administrator] AUTHORIZATION dbo');
IF DATABASE_PRINCIPAL_ID(N'User') IS NULL
    EXEC(N'CREATE ROLE [User] AUTHORIZATION dbo');

GRANT SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.[Товары] TO [Administrator];
GRANT SELECT ON OBJECT::dbo.[Товары] TO [User];
DENY INSERT, UPDATE, DELETE ON OBJECT::dbo.[Товары] TO [User];
GO
