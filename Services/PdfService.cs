using System.Reflection;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace QuanLyMayBay.Services;

public static class PdfService
{
    private sealed class FontResolver : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            => new("DejaVuSans", isBold, isItalic);
        public byte[] GetFont(string faceName)
        {
            using var stream = typeof(PdfService).Assembly.GetManifestResourceStream("DejaVuSans.ttf")!;
            using var output = new MemoryStream(); stream.CopyTo(output); return output.ToArray();
        }
    }
    private static readonly object FontLock = new();

    public static void XuatBang(string path, string title, IReadOnlyList<string> headers,
        IReadOnlyList<string[]> rows, string donVi = "Quản lý vé máy bay")
    {
        if (headers.Count == 0 || rows.Any(x => x.Length != headers.Count)) throw new ArgumentException("Bảng PDF không hợp lệ.");
        lock (FontLock) GlobalFontSettings.FontResolver ??= new FontResolver();
        using var document = new PdfDocument();
        document.Info.Title = title;
        var font = new XFont("AppDejaVu", 8);
        var bold = new XFont("AppDejaVu", 9, XFontStyleEx.Bold);
        XGraphics? gfx = null;
        PdfPage? page = null;
        double y = 0, width = 0;
        const double margin = 30, lineHeight = 13;
        void Draw(string text, XFont f, double x, double yy) => gfx!.DrawString(text, f, XBrushes.Black, x, yy);
        void NewPage()
        {
            gfx?.Dispose();
            page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            if (headers.Count > 5) page.Orientation = PdfSharp.PageOrientation.Landscape;
            gfx = XGraphics.FromPdfPage(page);
            width = (page.Width.Point - 2 * margin) / headers.Count;
            y = margin + 12;
            foreach (var line in Wrap(donVi, bold, page.Width.Point - 2 * margin)) { Draw(line, bold, margin, y); y += 14; }
            foreach (var line in title.Split('\n')) { Draw(line, bold, margin, y); y += 16; }
            var headerLines = headers.Select(x => Wrap(x, bold, width - 8)).ToArray();
            var headerHeight = headerLines.Max(x => x.Count) * lineHeight + 6;
            for (int i = 0; i < headers.Count; i++)
            {
                gfx.DrawRectangle(XPens.Gray, margin + i * width, y, width, headerHeight);
                var lines = headerLines[i];
                for (int n = 0; n < lines.Count; n++) Draw(lines[n], bold, margin + i * width + 4, y + 11 + n * lineHeight);
            }
            y += headerHeight;
            Draw("Trang " + document.PageCount, font, margin, page.Height.Point - 15);
        }
        List<string> Wrap(string text, XFont f, double max)
        {
            var lines = new List<string>();
            foreach (var paragraph in text.Replace("\r", "").Split('\n'))
            {
                var line = "";
                foreach (var token in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    var candidate = line.Length == 0 ? token : line + " " + token;
                    if (gfx!.MeasureString(candidate, f).Width <= max) { line = candidate; continue; }
                    if (line.Length > 0) { lines.Add(line); line = ""; }
                    foreach (var c in token)
                    {
                        if (line.Length > 0 && gfx.MeasureString(line + c, f).Width > max) { lines.Add(line); line = ""; }
                        line += c;
                    }
                }
                lines.Add(line);
            }
            return lines;
        }
        NewPage();
        foreach (var row in rows)
        {
            var cells = row.Select(x => Wrap(x ?? "", font, width - 8)).ToArray();
            int offset = 0, count = cells.Max(x => x.Count);
            while (offset < count)
            {
                int available = (int)((page!.Height.Point - margin - y - 6) / lineHeight);
                if (available < 1) { NewPage(); continue; }
                int length = Math.Min(count - offset, available);
                double height = length * lineHeight + 6;
                for (int i = 0; i < headers.Count; i++)
                {
                    gfx!.DrawRectangle(XPens.Gray, margin + i * width, y, width, height);
                    for (int n = 0; n < length && offset + n < cells[i].Count; n++)
                        Draw(cells[i][offset + n], font, margin + i * width + 4, y + 12 + n * lineHeight);
                }
                y += height; offset += length;
                if (offset < count) NewPage();
            }
        }
        gfx?.Dispose();
        document.Save(path);
    }
}
