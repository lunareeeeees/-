USE [master];
GO

IF DB_ID(N'ShopSimple') IS NULL
    CREATE DATABASE [ShopSimple];
GO

USE [ShopSimple];
GO

IF OBJECT_ID(N'dbo.Категории', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Категории
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Название NVARCHAR(100) NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Товары', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Товары
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Название NVARCHAR(100) NOT NULL,
        Цена DECIMAL(10,2) NOT NULL,
        Количество INT NOT NULL,
        КатегорияId INT NULL
    );
END;
GO

IF COL_LENGTH(N'dbo.Товары', N'КатегорияId') IS NULL
    ALTER TABLE dbo.Товары ADD КатегорияId INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Товары_Категории')
    ALTER TABLE dbo.Товары ADD CONSTRAINT FK_Товары_Категории
        FOREIGN KEY (КатегорияId) REFERENCES dbo.Категории(Id);
GO

IF OBJECT_ID(N'dbo.Поставщики', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Поставщики
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Название NVARCHAR(100) NOT NULL,
        Телефон NVARCHAR(30) NULL,
        Email NVARCHAR(100) NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Поставки', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Поставки
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ТоварId INT NOT NULL,
        ПоставщикId INT NULL,
        Количество INT NOT NULL,
        ДатаПоставки DATE NOT NULL,
        CONSTRAINT FK_Поставки_Товары FOREIGN KEY (ТоварId) REFERENCES dbo.Товары(Id),
        CONSTRAINT FK_Поставки_Поставщики FOREIGN KEY (ПоставщикId) REFERENCES dbo.Поставщики(Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.Продажи', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Продажи
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ТоварId INT NOT NULL,
        Количество INT NOT NULL,
        ДатаПродажи DATE NOT NULL,
        CONSTRAINT FK_Продажи_Товары FOREIGN KEY (ТоварId) REFERENCES dbo.Товары(Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.Пользователи', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Пользователи
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Логин NVARCHAR(50) NOT NULL UNIQUE,
        Пароль NVARCHAR(50) NOT NULL,
        Роль NVARCHAR(20) NOT NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Пользователи WHERE Логин = N'admin')
    INSERT INTO dbo.Пользователи (Логин, Пароль, Роль)
    VALUES (N'admin', N'1234', N'Администратор');

IF NOT EXISTS (SELECT 1 FROM dbo.Пользователи WHERE Логин = N'user')
    INSERT INTO dbo.Пользователи (Логин, Пароль, Роль)
    VALUES (N'user', N'1234', N'Пользователь');
GO
