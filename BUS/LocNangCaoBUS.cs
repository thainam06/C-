using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace VeMayBay.BUS;

public static class LocNangCaoBUS
{
    // Cu phap: DonGiaVe >= 1000000 AND NOT (TenHV ~ 'thuong gia').
    public static ArrayList Loc<T>(ArrayList nguon, string bieuThuc)
    {
        if (string.IsNullOrWhiteSpace(bieuThuc)) return new ArrayList(nguon);
        var parser = new Parser(typeof(T), bieuThuc);
        var dieuKien = parser.Parse();
        var ketQua = new ArrayList();
        foreach (var row in nguon) if (dieuKien(row)) ketQua.Add(row);
        return ketQua;
    }

    private sealed class Parser
    {
        private readonly List<string> tokens = new();
        private readonly Dictionary<string, PropertyInfo> fields;
        private int index;
        private int depth;

        public Parser(Type type, string source)
        {
            if (source.Length > 4096) throw new ArgumentException("Biểu thức quá dài.");
            fields = type.GetProperties().Where(x => x.CanRead && x.Name != "MatKhau")
                .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            var pattern = "\\G\\s*(>=|<=|<>|!=|[=><~()≥≤]|'(?:''|[^'])*'|\"(?:\"\"|[^\"])*\"|[\\p{L}_][\\p{L}\\p{N}_]*|[+-]?\\d+(?:\\.\\d+)?)";
            var pos = 0;
            var lexer = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
            while (pos < source.Length && !string.IsNullOrWhiteSpace(source[pos..]))
            {
                var match = lexer.Match(source, pos);
                if (!match.Success || match.Index != pos) throw new ArgumentException($"Sai cú pháp tại vị trí {pos + 1}.");
                tokens.Add(match.Groups[1].Value);
                pos += match.Length;
            }
        }

        private bool Is(string text) => index < tokens.Count && string.Equals(tokens[index], text, StringComparison.OrdinalIgnoreCase);
        private bool Take(string text) { if (!Is(text)) return false; index++; return true; }
        private string Next() => index < tokens.Count ? tokens[index++] : throw new ArgumentException("Biểu thức chưa đầy đủ.");

        public Func<object, bool> Parse()
        {
            var result = Or();
            if (index != tokens.Count) throw new ArgumentException("Thừa thành phần trong biểu thức.");
            return result;
        }

        private Func<object, bool> Or()
        {
            var left = And();
            while (Take("OR")) { var a = left; var b = And(); left = x => a(x) || b(x); }
            return left;
        }

        private Func<object, bool> And()
        {
            var left = Unary();
            while (Take("AND")) { var a = left; var b = Unary(); left = x => a(x) && b(x); }
            return left;
        }

        private Func<object, bool> Unary()
        {
            if (++depth > 32) throw new ArgumentException("Biểu thức lồng quá sâu.");
            try
            {
                if (Take("NOT")) { var a = Unary(); return x => !a(x); }
                if (Take("("))
                {
                    var result = Or();
                    if (!Take(")")) throw new ArgumentException("Thiếu dấu đóng ngoặc.");
                    return result;
                }
                var name = Next();
                if (!fields.TryGetValue(name, out var field)) throw new ArgumentException($"Không có trường được phép lọc: {name}.");
                var op = Next().Replace("≥", ">=").Replace("≤", "<=");
                if (!new[] { "=", "<>", "!=", ">", ">=", "<", "<=", "~" }.Contains(op))
                    throw new ArgumentException("Phép so sánh không hợp lệ.");
                var value = Next();
                if (value.Length >= 2 && (value[0] == '\'' || value[0] == '"'))
                    value = value[1..^1].Replace(new string(value[0], 2), value[0].ToString());
                var type = Nullable.GetUnderlyingType(field.PropertyType) ?? field.PropertyType;
                if (op == "~" && type != typeof(string)) throw new ArgumentException("Phép ~ chỉ dùng cho chuỗi.");
                object right;
                if (type == typeof(string)) right = value;
                else if (type == typeof(DateTime))
                {
                    if (!DateTime.TryParseExact(value, new[] { "yyyy-MM-dd", "dd/MM/yyyy" }, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var date)) throw new ArgumentException("Ngày lọc phải có dạng yyyy-MM-dd hoặc dd/MM/yyyy, đặt trong dấu nháy.");
                    right = date.Date;
                }
                else if (type == typeof(decimal) || type == typeof(int) || type == typeof(long) || type == typeof(double))
                {
                    if (!decimal.TryParse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture, out var number)) throw new ArgumentException("Giá trị số không hợp lệ; dùng dấu chấm thập phân.");
                    right = number;
                }
                else throw new ArgumentException("Kiểu dữ liệu này chưa hỗ trợ lọc.");
                return row =>
                {
                    var left = field.GetValue(row);
                    if (left == null) return false;
                    if (op == "~") return ((string)left).Contains((string)right, StringComparison.OrdinalIgnoreCase);
                    int comparison = type == typeof(string) ? string.Compare((string)left, (string)right, StringComparison.OrdinalIgnoreCase)
                        : type == typeof(DateTime) ? DateTime.Compare(((DateTime)left).Date, (DateTime)right)
                        : decimal.Compare(Convert.ToDecimal(left, CultureInfo.InvariantCulture), (decimal)right);
                    return op switch
                    {
                        "=" => comparison == 0, "<>" or "!=" => comparison != 0,
                        ">" => comparison > 0, ">=" => comparison >= 0,
                        "<" => comparison < 0, "<=" => comparison <= 0, _ => false
                    };
                };
            }
            finally { depth--; }
        }
    }
}
