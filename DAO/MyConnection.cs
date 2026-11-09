using Microsoft.Data.SqlClient;
using MySqlConnector;
using System.Data.Common;

namespace QuanLyMayBay.DAO;

public static class MyConnection
{
    public static bool DungMySql => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("QLMB_MYSQL_CONNECTION"));

    public static bool DaCauHinh => DungMySql || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("QLMB_SQL_CONNECTION"));

    public static SqlConnection MoKetNoi()
    {
        var chuoi = Environment.GetEnvironmentVariable("QLMB_SQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(chuoi))
            throw new InvalidOperationException("Chưa cấu hình QLMB_SQL_CONNECTION cho SQL Server.");
        var connection = new SqlConnection(chuoi);
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }

    // Tra ket noi CHUA mo, cung quy uoc voi mau DAO cua nhom.
    public static MySqlConnection GetConnection()
    {
        var chuoi = Environment.GetEnvironmentVariable("QLMB_MYSQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(chuoi))
            throw new InvalidOperationException("Chưa cấu hình QLMB_MYSQL_CONNECTION cho MySQL.");
        return new MySqlConnection(chuoi);
    }

    public static DbConnection MoKetNoiDuLieu()
    {
        if (!DungMySql) return MoKetNoi();
        var connection = GetConnection();
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }

    public static string KiemTra()
    {
        using var connection = MoKetNoiDuLieu();
        using var command = connection.CreateCommand();
        command.CommandText = DungMySql ? "SELECT DATABASE()" : "SELECT DB_NAME()";
        return Convert.ToString(command.ExecuteScalar()) ?? "";
    }
}
