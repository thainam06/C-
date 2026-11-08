using Microsoft.Data.SqlClient;
using QuanLyMayBay.DAO;
using System.Text.Json;

namespace QuanLyMayBay.Services;

public static class SqlSchemaService
{
    // Chi doc metadata, khong lay du lieu khach hang hay mat khau.
    public static void XuatCauTruc(string path)
    {
        using var connection = MyConnection.MoKetNoi();
        List<Dictionary<string, object?>> Query(string sql)
        {
            using var command = new SqlCommand(sql, connection);
            using var reader = command.ExecuteReader();
            var rows = new List<Dictionary<string, object?>>();
            while (reader.Read())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(row);
            }
            return rows;
        }
        var data = new
        {
            Database = connection.Database,
            Columns = Query("""
                SELECT s.name AS SchemaName,o.name AS ObjectName,o.type_desc AS ObjectType,
                  c.name AS ColumnName,ty.name AS DataType,c.max_length AS MaxLengthBytes,
                  c.precision AS NumericPrecision,c.scale AS NumericScale,c.is_nullable AS IsNullable,
                  c.is_identity AS IsIdentity,c.is_computed AS IsComputed
                FROM sys.objects o JOIN sys.schemas s ON s.schema_id=o.schema_id
                JOIN sys.columns c ON c.object_id=o.object_id JOIN sys.types ty ON ty.user_type_id=c.user_type_id
                WHERE o.type IN ('U','V') AND o.is_ms_shipped=0 ORDER BY s.name,o.name,c.column_id
                """),
            PrimaryKeys = Query("""
                SELECT s.name AS SchemaName,t.name AS TableName,c.name AS ColumnName,ic.key_ordinal AS KeyOrder
                FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
                JOIN sys.indexes i ON i.object_id=t.object_id AND i.is_primary_key=1
                JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
                JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
                ORDER BY s.name,t.name,ic.key_ordinal
                """),
            ForeignKeys = Query("""
                SELECT fk.name AS ConstraintName,ps.name AS ParentSchema,p.name AS ParentTable,pc.name AS ParentColumn,
                  rs.name AS ReferencedSchema,r.name AS ReferencedTable,rc.name AS ReferencedColumn
                FROM sys.foreign_keys fk JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id
                JOIN sys.tables p ON p.object_id=fkc.parent_object_id JOIN sys.schemas ps ON ps.schema_id=p.schema_id
                JOIN sys.columns pc ON pc.object_id=p.object_id AND pc.column_id=fkc.parent_column_id
                JOIN sys.tables r ON r.object_id=fkc.referenced_object_id JOIN sys.schemas rs ON rs.schema_id=r.schema_id
                JOIN sys.columns rc ON rc.object_id=r.object_id AND rc.column_id=fkc.referenced_column_id
                ORDER BY fk.name,fkc.constraint_column_id
                """)
        };
        if (data.Columns.Count == 0)
            throw new InvalidOperationException("Không thấy bảng/view người dùng. Cần kiểm tra đúng database và quyền xem metadata.");
        File.WriteAllText(path, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
    }
}
