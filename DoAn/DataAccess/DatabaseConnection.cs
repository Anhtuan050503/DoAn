using System.Data.SqlClient;

namespace DoAn.DataAccess
{
    public class DatabaseConnection
    {
        // Chuỗi kết nối đến máy của bạn
        private readonly string connectionString = @"Server=localhost\SQLEXPRESS;Database=QuanLyBenhVien;Trusted_Connection=True;";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}