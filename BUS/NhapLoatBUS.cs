namespace VeMayBay.BUS;

public static class NhapLoatBUS
{
    public static List<T> HopNhat<T>(IEnumerable<T> current, IEnumerable<T> rows, Func<T, string> key,
        Func<T, T> normalize, Action<T> validate)
    {
        var result = current.ToDictionary(key, normalize, StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int line = 1;
        foreach (var input in rows)
        {
            line++;
            try
            {
                var item = normalize(input); validate(item);
                if (!seen.Add(key(item))) throw new ArgumentException("Trùng mã trong file nhập.");
                result[key(item)] = item;
            }
            catch (ArgumentException ex) { throw new ArgumentException($"Dòng {line}: {ex.Message}"); }
        }
        return result.Values.ToList();
    }
}
