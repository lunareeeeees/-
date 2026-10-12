using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ShopSimpleWpf
{
    public class Database
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["Shop"].ConnectionString;

        public string Login(string login, string password)
        {
            string sql = "SELECT Роль FROM dbo.Пользователи " +
                         "WHERE Логин=@login AND Пароль=@password";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@login", login);
                command.Parameters.AddWithValue("@password", password);
                connection.Open();

                object result = command.ExecuteScalar();
                return result == null ? null : result.ToString();
            }
        }

        public DataTable GetProducts()
        {
            return GetData(@"SELECT t.Id, t.Название, t.Цена, t.Количество,
                            t.КатегорияId, c.Название AS Категория
                            FROM dbo.Товары t
                            LEFT JOIN dbo.Категории c ON c.Id=t.КатегорияId
                            ORDER BY t.Id");
        }

        public DataTable GetStatistics()
        {
            return GetData("SELECT * FROM dbo.vw_ОбщаяСтатистика");
        }

        public DataTable GetProductSummary()
        {
            return GetData("SELECT * FROM dbo.vw_СводкаТоваров ORDER BY Товар");
        }

        public void AddProduct(string name, decimal price, int count, int categoryId)
        {
            Run(@"INSERT INTO dbo.Товары (Название, Цена, Количество, КатегорияId)
                  VALUES (@name, @price, @count, @category)",
                P("@name", name), P("@price", price), P("@count", count),
                P("@category", categoryId));
        }

        public void EditProduct(int id, string name, decimal price, int count, int categoryId)
        {
            Run(@"UPDATE dbo.Товары SET Название=@name, Цена=@price,
                  Количество=@count, КатегорияId=@category WHERE Id=@id",
                P("@id", id), P("@name", name), P("@price", price),
                P("@count", count), P("@category", categoryId));
        }

        public void DeleteProduct(int id)
        {
            Run("DELETE FROM dbo.Товары WHERE Id=@id", P("@id", id));
        }

        public DataTable GetCategories()
        {
            return GetData("SELECT Id, Название FROM dbo.Категории ORDER BY Название");
        }

        public void AddCategory(string name)
        {
            Run("INSERT INTO dbo.Категории (Название) VALUES (@name)", P("@name", name));
        }

        public void EditCategory(int id, string name)
        {
            Run("UPDATE dbo.Категории SET Название=@name WHERE Id=@id",
                P("@id", id), P("@name", name));
        }

        public void DeleteCategory(int id)
        {
            Run("DELETE FROM dbo.Категории WHERE Id=@id", P("@id", id));
        }

        public DataTable GetSuppliers()
        {
            return GetData("SELECT Id, Название, Телефон, Email FROM dbo.Поставщики ORDER BY Название");
        }

        public void AddSupplier(string name, string phone, string email)
        {
            Run(@"INSERT INTO dbo.Поставщики (Название, Телефон, Email)
                  VALUES (@name, @phone, @email)",
                P("@name", name), P("@phone", phone), P("@email", email));
        }

        public void EditSupplier(int id, string name, string phone, string email)
        {
            Run(@"UPDATE dbo.Поставщики SET Название=@name, Телефон=@phone,
                  Email=@email WHERE Id=@id",
                P("@id", id), P("@name", name), P("@phone", phone), P("@email", email));
        }

        public void DeleteSupplier(int id)
        {
            Run("DELETE FROM dbo.Поставщики WHERE Id=@id", P("@id", id));
        }

        public DataTable GetSupplies()
        {
            return GetData(@"SELECT p.Id, p.ТоварId, t.Название AS Товар,
                            p.ПоставщикId, s.Название AS Поставщик,
                            p.Количество, p.ДатаПоставки
                            FROM dbo.Поставки p
                            INNER JOIN dbo.Товары t ON t.Id=p.ТоварId
                            LEFT JOIN dbo.Поставщики s ON s.Id=p.ПоставщикId
                            ORDER BY p.Id");
        }

        public void AddSupply(int productId, int supplierId, int count, DateTime date)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        RunInTransaction(connection, transaction,
                            @"INSERT INTO dbo.Поставки
                              (ТоварId, ПоставщикId, Количество, ДатаПоставки)
                              VALUES (@product, @supplier, @count, @date)",
                            P("@product", productId), P("@supplier", supplierId),
                            P("@count", count), P("@date", date));

                        RunInTransaction(connection, transaction,
                            @"UPDATE dbo.Товары SET Количество=Количество+@count
                              WHERE Id=@product",
                            P("@product", productId), P("@count", count));

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void EditSupply(int id, int productId, int supplierId, int count, DateTime date)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int oldProductId;
                        int oldCount;

                        using (var command = new SqlCommand(
                            "SELECT ТоварId, Количество FROM dbo.Поставки WHERE Id=@id",
                            connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", id);
                            using (var reader = command.ExecuteReader())
                            {
                                if (!reader.Read())
                                    throw new InvalidOperationException("Поставка не найдена.");

                                oldProductId = reader.GetInt32(0);
                                oldCount = reader.GetInt32(1);
                            }
                        }

                        if (oldProductId == productId)
                        {
                            int difference = count - oldCount;
                            if (difference >= 0)
                            {
                                RunInTransaction(connection, transaction,
                                    @"UPDATE dbo.Товары SET Количество=Количество+@count
                                      WHERE Id=@product",
                                    P("@product", productId), P("@count", difference));
                            }
                            else
                            {
                                int decrease = -difference;
                                int changed = RunInTransaction(connection, transaction,
                                    @"UPDATE dbo.Товары SET Количество=Количество-@count
                                      WHERE Id=@product AND Количество>=@count",
                                    P("@product", productId), P("@count", decrease));

                                if (changed == 0)
                                    throw new InvalidOperationException(
                                        "Нельзя уменьшить поставку: товара не хватает на складе.");
                            }
                        }
                        else
                        {
                            int changed = RunInTransaction(connection, transaction,
                                @"UPDATE dbo.Товары SET Количество=Количество-@count
                                  WHERE Id=@product AND Количество>=@count",
                                P("@product", oldProductId), P("@count", oldCount));

                            if (changed == 0)
                                throw new InvalidOperationException(
                                    "Нельзя изменить товар поставки: часть товара уже продана.");

                            RunInTransaction(connection, transaction,
                                @"UPDATE dbo.Товары SET Количество=Количество+@count
                                  WHERE Id=@product",
                                P("@product", productId), P("@count", count));
                        }

                        RunInTransaction(connection, transaction,
                            @"UPDATE dbo.Поставки SET ТоварId=@product,
                              ПоставщикId=@supplier, Количество=@count,
                              ДатаПоставки=@date WHERE Id=@id",
                            P("@id", id), P("@product", productId), P("@supplier", supplierId),
                            P("@count", count), P("@date", date));

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void DeleteSupply(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int productId;
                        int count;

                        using (var command = new SqlCommand(
                            "SELECT ТоварId, Количество FROM dbo.Поставки WHERE Id=@id",
                            connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", id);
                            using (var reader = command.ExecuteReader())
                            {
                                if (!reader.Read())
                                    throw new InvalidOperationException("Поставка не найдена.");

                                productId = reader.GetInt32(0);
                                count = reader.GetInt32(1);
                            }
                        }

                        int changed = RunInTransaction(connection, transaction,
                            @"UPDATE dbo.Товары SET Количество=Количество-@count
                              WHERE Id=@product AND Количество>=@count",
                            P("@product", productId), P("@count", count));

                        if (changed == 0)
                            throw new InvalidOperationException(
                                "Нельзя удалить поставку: часть этого товара уже продана.");

                        RunInTransaction(connection, transaction,
                            "DELETE FROM dbo.Поставки WHERE Id=@id", P("@id", id));

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public DataTable GetSales()
        {
            return GetData(@"SELECT s.Id, s.ТоварId, t.Название AS Товар,
                            s.Количество, s.ЦенаПродажи,
                            CAST(s.Количество*s.ЦенаПродажи AS DECIMAL(14,2)) AS Сумма,
                            s.ДатаПродажи
                            FROM dbo.Продажи s
                            INNER JOIN dbo.Товары t ON t.Id=s.ТоварId
                            ORDER BY s.Id");
        }

        public void AddSale(int productId, int count, DateTime date)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        decimal price = GetProductPrice(connection, transaction, productId);

                        int changed = RunInTransaction(connection, transaction,
                            @"UPDATE dbo.Товары SET Количество=Количество-@count
                              WHERE Id=@product AND Количество>=@count",
                            P("@product", productId), P("@count", count));

                        if (changed == 0)
                            throw new InvalidOperationException("Недостаточно товара на складе.");

                        RunInTransaction(connection, transaction,
                            @"INSERT INTO dbo.Продажи
                              (ТоварId, Количество, ЦенаПродажи, ДатаПродажи)
                              VALUES (@product, @count, @price, @date)",
                            P("@product", productId), P("@count", count),
                            P("@price", price), P("@date", date));

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void EditSale(int id, int productId, int count, DateTime date)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int oldProductId;
                        int oldCount;

                        using (var command = new SqlCommand(
                            "SELECT ТоварId, Количество FROM dbo.Продажи WHERE Id=@id",
                            connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", id);
                            using (var reader = command.ExecuteReader())
                            {
                                if (!reader.Read())
                                    throw new InvalidOperationException("Продажа не найдена.");

                                oldProductId = reader.GetInt32(0);
                                oldCount = reader.GetInt32(1);
                            }
                        }

                        RunInTransaction(connection, transaction,
                            @"UPDATE dbo.Товары SET Количество=Количество+@count
                              WHERE Id=@product",
                            P("@product", oldProductId), P("@count", oldCount));

                        decimal price = GetProductPrice(connection, transaction, productId);

                        int changed = RunInTransaction(connection, transaction,
                            @"UPDATE dbo.Товары SET Количество=Количество-@count
                              WHERE Id=@product AND Количество>=@count",
                            P("@product", productId), P("@count", count));

                        if (changed == 0)
                            throw new InvalidOperationException("Недостаточно товара на складе.");

                        RunInTransaction(connection, transaction,
                            @"UPDATE dbo.Продажи SET ТоварId=@product,
                              Количество=@count, ЦенаПродажи=@price,
                              ДатаПродажи=@date WHERE Id=@id",
                            P("@id", id), P("@product", productId), P("@count", count),
                            P("@price", price), P("@date", date));

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void DeleteSale(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int productId;
                        int count;

                        using (var command = new SqlCommand(
                            "SELECT ТоварId, Количество FROM dbo.Продажи WHERE Id=@id",
                            connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", id);
                            using (var reader = command.ExecuteReader())
                            {
                                if (!reader.Read())
                                    throw new InvalidOperationException("Продажа не найдена.");

                                productId = reader.GetInt32(0);
                                count = reader.GetInt32(1);
                            }
                        }

                        RunInTransaction(connection, transaction,
                            @"UPDATE dbo.Товары SET Количество=Количество+@count
                              WHERE Id=@product",
                            P("@product", productId), P("@count", count));

                        RunInTransaction(connection, transaction,
                            "DELETE FROM dbo.Продажи WHERE Id=@id", P("@id", id));

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private DataTable GetData(string sql)
        {
            var table = new DataTable();

            using (var connection = new SqlConnection(connectionString))
            using (var adapter = new SqlDataAdapter(sql, connection))
            {
                adapter.Fill(table);
            }

            return table;
        }

        private void Run(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddRange(parameters);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private int RunInTransaction(SqlConnection connection, SqlTransaction transaction,
            string sql, params SqlParameter[] parameters)
        {
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddRange(parameters);
                return command.ExecuteNonQuery();
            }
        }

        private decimal GetProductPrice(SqlConnection connection,
            SqlTransaction transaction, int productId)
        {
            using (var command = new SqlCommand(
                "SELECT Цена FROM dbo.Товары WHERE Id=@id", connection, transaction))
            {
                command.Parameters.AddWithValue("@id", productId);
                object result = command.ExecuteScalar();

                if (result == null)
                    throw new InvalidOperationException("Товар не найден.");

                return Convert.ToDecimal(result);
            }
        }

        private SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }
    }
}
