using System.Globalization;
using System.Reflection;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace VeMayBay.Services;

public static class ExcelService
{
    public static void Xuat<T>(string path, IEnumerable<T> data, IEnumerable<string>? columns = null)
    {
        var fields = typeof(T).GetProperties().Where(x => x.CanRead && x.CanWrite && x.Name != "MatKhau"
            && (columns == null || columns.Contains(x.Name))).ToArray();
        var rows = data.ToList();
        if (rows.Count > 65535 || fields.Length == 0) throw new ArgumentException("File XLS tối đa 65.535 dòng dữ liệu và cần ít nhất một cột.");
        using var workbook = new HSSFWorkbook();
        var sheet = workbook.CreateSheet("DuLieu");
        var header = sheet.CreateRow(0);
        var style = workbook.CreateCellStyle();
        var font = workbook.CreateFont(); font.IsBold = true; style.SetFont(font);
        for (int i = 0; i < fields.Length; i++)
        {
            header.CreateCell(i).SetCellValue(fields[i].Name);
            header.GetCell(i).CellStyle = style;
            sheet.SetColumnWidth(i, 24 * 256);
        }
        for (int i = 0; i < rows.Count; i++)
        {
            var row = sheet.CreateRow(i + 1);
            for (int j = 0; j < fields.Length; j++)
            {
                var value = fields[j].GetValue(rows[i]);
                var cell = row.CreateCell(j);
                if (value is double real && !double.IsFinite(real)) throw new ArgumentException("Không xuất NaN hoặc vô cực vào Excel.");
                if (value is DateTime date) cell.SetCellValue(date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                else if (value is decimal number && number.ToString(CultureInfo.InvariantCulture).Count(char.IsDigit) > 15)
                    cell.SetCellValue(number.ToString(CultureInfo.InvariantCulture));
                else if (value is decimal or double or int or long) cell.SetCellValue(Convert.ToDouble(value, CultureInfo.InvariantCulture));
                else cell.SetCellValue(Convert.ToString(value, CultureInfo.InvariantCulture) ?? "");
            }
        }
        sheet.CreateFreezePane(0, 1);
        using var stream = File.Create(path);
        workbook.Write(stream, leaveOpen: true);
    }

    public static List<T> Nhap<T>(string path) where T : new()
    {
        if (new FileInfo(path).Length > 20 * 1024 * 1024) throw new ArgumentException("File Excel quá 20 MB.");
        using var stream = File.OpenRead(path);
        using var workbook = WorkbookFactory.Create(stream);
        if (workbook.NumberOfSheets == 0) throw new ArgumentException("File không có sheet.");
        var sheet = workbook.GetSheetAt(0);
        if (sheet.LastRowNum > 65535) throw new ArgumentException("Chỉ nhập tối đa 65.535 dòng dữ liệu mỗi lần.");
        var header = sheet.GetRow(0) ?? throw new ArgumentException("Thiếu hàng tiêu đề.");
        var properties = typeof(T).GetProperties().Where(x => x.CanWrite).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        var columns = new List<(int Column, PropertyInfo Field)>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < header.LastCellNum; i++)
        {
            var name = header.GetCell(i)?.ToString()?.Trim() ?? "";
            if (name.Length == 0) continue;
            if (name.Equals("MatKhau", StringComparison.OrdinalIgnoreCase) || !names.Add(name) || !properties.TryGetValue(name, out var property))
                throw new ArgumentException($"Cột không hợp lệ, bị lặp hoặc không được nhập: {name}.");
            columns.Add((i, property));
        }
        if (columns.Count == 0) throw new ArgumentException("Không có cột dữ liệu phù hợp.");
        var missing = properties.Keys.Where(x => x != "MatKhau" && !names.Contains(x)).ToArray();
        if (missing.Length > 0) throw new ArgumentException("Thiếu cột: " + string.Join(", ", missing));
        var result = new List<T>();
        for (int i = 1; i <= sheet.LastRowNum; i++)
        {
            var row = sheet.GetRow(i);
            if (row == null || row.Cells.All(c => string.IsNullOrWhiteSpace(c.ToString()))) continue;
            var item = new T();
            foreach (var column in columns)
            {
                var cell = row.GetCell(column.Column);
                if (cell?.CellType == CellType.Formula) throw new ArgumentException($"Dòng {i + 1}: không nhập công thức Excel.");
                var text = cell?.ToString()?.Trim() ?? "";
                var type = Nullable.GetUnderlyingType(column.Field.PropertyType) ?? column.Field.PropertyType;
                try
                {
                    object? value;
                    if (type == typeof(string)) value = text;
                    else if (string.IsNullOrEmpty(text))
                        value = Nullable.GetUnderlyingType(column.Field.PropertyType) != null ? null
                            : throw new FormatException("Ô số/ngày không được trống.");
                    else if (type == typeof(DateTime))
                    {
                        if (cell?.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
                            value = cell.DateCellValue;
                        else value = DateTime.ParseExact(text, new[] { "dd/MM/yyyy", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.None);
                    }
                    else if (cell?.CellType == CellType.Numeric) value = Convert.ChangeType(cell.NumericCellValue, type, CultureInfo.InvariantCulture);
                    else value = Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
                    if (value is double real && !double.IsFinite(real)) throw new FormatException("Không nhận NaN hoặc vô cực.");
                    column.Field.SetValue(item, value);
                }
                catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException)
                { throw new ArgumentException($"Dòng {i + 1}, cột {column.Field.Name}: giá trị không đúng kiểu dữ liệu."); }
            }
            result.Add(item);
        }
        return result;
    }
}
