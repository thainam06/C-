using Microsoft.Data.SqlClient;

namespace QuanLyMayBay.DAO;

public static class MyConnection
{
    public static bool DaCauHinh => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("QLMB_SQL_CONNECTION"));

    public static SqlConnection MoKetNoi()
    {
        var chuoi = Environment.GetEnvironmentVariable("QLMB_SQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(chuoi))
            throw new InvalidOperationException("Chưa cấu hình QLMB_SQL_CONNECTION cho SQL Server.");
        var connection = new SqlConnection(chuoi);
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }

    public static string KiemTra()
    {
        using var connection = MoKetNoi();
        using var command = new SqlCommand("SELECT DB_NAME()", connection);
        return Convert.ToString(command.ExecuteScalar()) ?? "";
    }
}
