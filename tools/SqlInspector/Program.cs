using QuanLyMayBay.DAO;
using QuanLyMayBay.Services;
using Microsoft.Data.SqlClient;

namespace QuanLyMayBay.Tools;

internal static class SqlInspector
{
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2 || args[0] != "--schema")
            {
                Console.WriteLine("Su dung: dotnet run --project tools/SqlInspector/SqlInspector.csproj -- --schema duong-dan.json");
                return 2;
            }
            Console.WriteLine("Kiem tra ket noi, chi doc metadata...");
            var database = MyConnection.KiemTra();
            SqlSchemaService.XuatCauTruc(args[1]);
            Console.WriteLine($"Da xuat cau truc CSDL {database}. Khong xuat du lieu nguoi dung hay chuoi ket noi.");
            return 0;
        }
        catch (SqlException ex)
        {
            Console.Error.WriteLine($"SQL error {ex.Number}: {ex.Message}");
            Console.Error.WriteLine("Khong in chuoi ket noi. Neu loi chung chi, cau hinh chung chi duoc tin cay thay vi tat kiem tra TLS.");
            return 1;
        }
        catch
        {
            Console.Error.WriteLine("Chua ket noi/xuat duoc metadata. Kiem tra server, quyen doc metadata, bien QLMB_SQL_CONNECTION va chung chi TLS. Khong doi TrustServerCertificate thanh True de bo qua loi.");
            return 1;
        }
    }
}
