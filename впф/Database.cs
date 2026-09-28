using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ShopSimpleWpf
{
    public class Database
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["Shop"].ConnectionString;

        public DataTable GetProducts()
        {
            var table = new DataTable();
            var sql = "SELECT Id, Название, Цена, Количество FROM dbo.Товары";

            using (var connection = new SqlConnection(connectionString))
            using (var adapter = new SqlDataAdapter(sql, connection))
            {
                adapter.Fill(table);
            }

            return table;
        }

        public void AddProduct(string name, decimal price, int count)
        {
            var sql = "INSERT INTO dbo.Товары (Название, Цена, Количество) " +
                      "VALUES (@name, @price, @count)";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@price", price);
                command.Parameters.AddWithValue("@count", count);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void EditProduct(int id, string name, decimal price, int count)
        {
            var sql = "UPDATE dbo.Товары SET Название=@name, Цена=@price, " +
                      "Количество=@count WHERE Id=@id";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@price", price);
                command.Parameters.AddWithValue("@count", count);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteProduct(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(
                "DELETE FROM dbo.Товары WHERE Id=@id", connection))
            {
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

    }
}
