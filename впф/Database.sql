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
        КатегорияId INT NOT NULL
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
        ПоставщикId INT NOT NULL,
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
        ЦенаПродажи DECIMAL(10,2) NOT NULL,
        ДатаПродажи DATE NOT NULL,
        CONSTRAINT FK_Продажи_Товары FOREIGN KEY (ТоварId) REFERENCES dbo.Товары(Id)
    );
END;
GO

-- Обновление старой версии базы: все товары должны иметь категорию.
IF EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Товары')
      AND name = N'КатегорияId' AND is_nullable = 1
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Категории WHERE Название = N'Без категории')
        INSERT INTO dbo.Категории (Название) VALUES (N'Без категории');

    DECLARE @КатегорияПоУмолчанию INT;
    SELECT TOP 1 @КатегорияПоУмолчанию = Id
    FROM dbo.Категории WHERE Название = N'Без категории';

    UPDATE dbo.Товары
    SET КатегорияId = @КатегорияПоУмолчанию
    WHERE КатегорияId IS NULL;

    ALTER TABLE dbo.Товары ALTER COLUMN КатегорияId INT NOT NULL;
END;
GO

-- Обновление старой версии базы: каждая поставка должна иметь поставщика.
IF COL_LENGTH(N'dbo.Поставки', N'ПоставщикId') IS NULL
    ALTER TABLE dbo.Поставки ADD ПоставщикId INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Поставки_Поставщики')
    ALTER TABLE dbo.Поставки ADD CONSTRAINT FK_Поставки_Поставщики
        FOREIGN KEY (ПоставщикId) REFERENCES dbo.Поставщики(Id);
GO

IF EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Поставки')
      AND name = N'ПоставщикId' AND is_nullable = 1
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Поставщики WHERE Название = N'Не указан')
        INSERT INTO dbo.Поставщики (Название) VALUES (N'Не указан');

    DECLARE @ПоставщикПоУмолчанию INT;
    SELECT TOP 1 @ПоставщикПоУмолчанию = Id
    FROM dbo.Поставщики WHERE Название = N'Не указан';

    UPDATE dbo.Поставки
    SET ПоставщикId = @ПоставщикПоУмолчанию
    WHERE ПоставщикId IS NULL;

    ALTER TABLE dbo.Поставки ALTER COLUMN ПоставщикId INT NOT NULL;
END;
GO

-- Цена сохраняется в продаже, чтобы старая выручка не менялась
-- после изменения текущей цены товара.
IF COL_LENGTH(N'dbo.Продажи', N'ЦенаПродажи') IS NULL
    ALTER TABLE dbo.Продажи ADD ЦенаПродажи DECIMAL(10,2) NULL;
GO

UPDATE s
SET ЦенаПродажи = t.Цена
FROM dbo.Продажи s
INNER JOIN dbo.Товары t ON t.Id = s.ТоварId
WHERE s.ЦенаПродажи IS NULL;
GO

IF EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Продажи')
      AND name = N'ЦенаПродажи' AND is_nullable = 1
)
    ALTER TABLE dbo.Продажи ALTER COLUMN ЦенаПродажи DECIMAL(10,2) NOT NULL;
GO

-- Простые проверки защищают базу от отрицательных значений.
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Товары_Цена')
    ALTER TABLE dbo.Товары ADD CONSTRAINT CK_Товары_Цена CHECK (Цена >= 0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Товары_Количество')
    ALTER TABLE dbo.Товары ADD CONSTRAINT CK_Товары_Количество CHECK (Количество >= 0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Поставки_Количество')
    ALTER TABLE dbo.Поставки ADD CONSTRAINT CK_Поставки_Количество CHECK (Количество > 0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Продажи_Количество')
    ALTER TABLE dbo.Продажи ADD CONSTRAINT CK_Продажи_Количество CHECK (Количество > 0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Продажи_Цена')
    ALTER TABLE dbo.Продажи ADD CONSTRAINT CK_Продажи_Цена CHECK (ЦенаПродажи >= 0);
GO

-- Сводка связывает товар, категорию, поставки и продажи.
CREATE OR ALTER VIEW dbo.vw_СводкаТоваров
AS
SELECT
    t.Id,
    t.Название AS Товар,
    c.Название AS Категория,
    t.Цена,
    t.Количество AS Остаток,
    ISNULL(p.Поставлено, 0) AS Поставлено,
    ISNULL(s.Продано, 0) AS Продано,
    CAST(t.Цена * t.Количество AS DECIMAL(14,2)) AS СтоимостьОстатка,
    p.ПоследняяПоставка,
    s.ПоследняяПродажа
FROM dbo.Товары t
INNER JOIN dbo.Категории c ON c.Id = t.КатегорияId
LEFT JOIN
(
    SELECT ТоварId, SUM(Количество) AS Поставлено,
           MAX(ДатаПоставки) AS ПоследняяПоставка
    FROM dbo.Поставки
    GROUP BY ТоварId
) p ON p.ТоварId = t.Id
LEFT JOIN
(
    SELECT ТоварId, SUM(Количество) AS Продано,
           MAX(ДатаПродажи) AS ПоследняяПродажа
    FROM dbo.Продажи
    GROUP BY ТоварId
) s ON s.ТоварId = t.Id;
GO

CREATE OR ALTER VIEW dbo.vw_ОбщаяСтатистика
AS
SELECT
    (SELECT COUNT(*) FROM dbo.Товары) AS НаименованийТоваров,
    ISNULL((SELECT SUM(Количество) FROM dbo.Товары), 0) AS ЕдиницНаСкладе,
    (SELECT COUNT(*) FROM dbo.Категории) AS Категорий,
    (SELECT COUNT(*) FROM dbo.Поставщики) AS Поставщиков,
    ISNULL((SELECT SUM(Количество) FROM dbo.Поставки), 0) AS ПоставленоЕдиниц,
    ISNULL((SELECT SUM(Количество) FROM dbo.Продажи), 0) AS ПроданоЕдиниц,
    ISNULL((SELECT SUM(Количество * ЦенаПродажи) FROM dbo.Продажи), 0) AS Выручка,
    ISNULL((SELECT SUM(Количество * Цена) FROM dbo.Товары), 0) AS СтоимостьОстатка;
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
