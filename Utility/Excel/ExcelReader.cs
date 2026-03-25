using ClosedXML.Excel;

namespace Utility.Excel;

public static class ExcelReader
{
    public static Dictionary<string, DataMatrix> Read(Stream stream) =>
        Read(new XLWorkbook(stream));

    public static Dictionary<string, DataMatrix> Read(string filePath) =>
        Read(new XLWorkbook(filePath));

    private static Dictionary<string, DataMatrix> Read(IXLWorkbook workbook) =>
        Enumerable.Range(1, workbook.Worksheets.Count)
            .Select(workbook.Worksheet)
            .Where(sheet => sheet.Tables.Any())
            .ToDictionary(
                sheet => sheet.Name,
                sheet =>
                {
                    var table = sheet.Tables.First();
                    var keys = table.Fields.Select(field => field.Name).ToList();
                    var dic = table.DataRange.Rows().Select(row =>
                        keys.ToDictionary(key => key, key =>
                        {
                            var xlCellValue = row.Field(key).Value;
                            return xlCellValue.Type switch
                            {
                                XLDataType.Blank => (object)null!,
                                XLDataType.Boolean => xlCellValue.GetBoolean(),
                                XLDataType.Number => xlCellValue.GetNumber(),
                                XLDataType.Text => xlCellValue.GetText(),
                                XLDataType.Error => xlCellValue.GetError(),
                                XLDataType.DateTime => xlCellValue.GetDateTime(),
                                XLDataType.TimeSpan => xlCellValue.GetTimeSpan(),
                                _ => throw new InvalidCastException()
                            };
                        })).ToList();
                    return new DataMatrix { Keys = keys, Rows = dic };
                }
            );
}