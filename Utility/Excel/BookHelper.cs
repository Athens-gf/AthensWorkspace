using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace Utility.Excel;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class BookHelper
{
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [GeneratedRegex(@"[\\/\?\*\[\]:]")]
    private static partial Regex RegInvalidChar();

    public XLWorkbook Workbook { get; set; }

    public BookHelper()
    {
        Workbook = new XLWorkbook();
        Workbook.Style.Font.SetFontName("游ゴシック");
    }

    public byte[] SaveAsToByte()
    {
        using var memory = new MemoryStream();
        Workbook.SaveAs(memory);
        return memory.ToArray();
    }

    public static string SanitizeSheetName(string name, string defName = "Sheet1")
    {
        if (string.IsNullOrWhiteSpace(name)) return defName;
        var sanitized = RegInvalidChar().Replace(name, "_").Trim('\'');
        if (sanitized.Length > 31) sanitized = sanitized[..31];
        if (sanitized.Equals("History", StringComparison.OrdinalIgnoreCase)) sanitized = "History_";
        return string.IsNullOrWhiteSpace(sanitized) ? defName : sanitized;
    }

    public SheetHelper GetSheet(string sheetName)
    {
        var sanitizedSheetName = SanitizeSheetName(sheetName);
        if (!Workbook.TryGetWorksheet(sanitizedSheetName, out var worksheet))
            worksheet = Workbook.Worksheets.Add(sanitizedSheetName);
        return new SheetHelper(worksheet);
    }

    public FileContentResult File(string fileName) => new(SaveAsToByte(), ExcelContentType) { FileDownloadName = fileName };
}