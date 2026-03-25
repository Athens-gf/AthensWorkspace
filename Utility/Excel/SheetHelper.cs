using System.Diagnostics.CodeAnalysis;
using ClosedXML.Excel;

namespace Utility.Excel;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class SheetHelper(IXLWorksheet worksheet)
{
    public IXLWorksheet Worksheet => worksheet;
    public bool IsHorizontal { get; set; } = true;
    public List<string> Header { get; set; } = [];
    public List<Dictionary<string, XLCellValue>> Data { get; set; } = [];


    public void SetHeader(IEnumerable<string> header) => Header = header.ToList();

    public void SetData<T>(IEnumerable<T> data, Action<T, Dictionary<string, XLCellValue>> setDic)
    {
        Data.Clear();
        foreach (var value in data)
        {
            var dic = new Dictionary<string, XLCellValue>();
            setDic(value, dic);
            Data.Add(dic);
        }
    }

    public void SetData(Action<Dictionary<string, XLCellValue>> setDic)
    {
        Data.Clear();
        var dic = new Dictionary<string, XLCellValue>();
        setDic(dic);
        Data.Add(dic);
    }

    public void Write()
    {
        if (IsHorizontal)
        {
            var headerRow = Worksheet.Row(1);
            foreach (var (value, ci) in Header.ZipWithIndex())
                headerRow.Cell(ci + 1).SetValue(value);

            foreach (var (dic, ri) in Data.ZipWithIndex())
            {
                var row = Worksheet.Row(ri + 2);
                foreach (var (header, ci) in Header.ZipWithIndex())
                {
                    if (!dic.TryGetValue(header, out var value)) continue;
                    row.Cell(ci + 1).SetValue(value);
                }
            }
        }
        else
        {
            var headerColumn = Worksheet.Column(1);
            foreach (var (value, ri) in Header.ZipWithIndex())
                headerColumn.Cell(ri + 1).SetValue(value);

            foreach (var (dic, ci) in Data.ZipWithIndex())
            {
                var col = Worksheet.Column(ci + 2);
                foreach (var (header, ri) in Header.ZipWithIndex())
                {
                    if (!dic.TryGetValue(header, out var value)) continue;
                    col.Cell(ri + 1).SetValue(value);
                }
            }
        }
    }

    protected void WriteAsTable(IEnumerable<string> header, int dataList)
    {
        var headerList = header.ToList();
        SetHeader(headerList);
        Write();
        var firstCell = Worksheet.Cell(1, 1);
        var lastCell = IsHorizontal ? Worksheet.Cell(dataList + 1, headerList.Count) : Worksheet.Cell(headerList.Count, dataList + 1);
        Worksheet.Tables.Add(Worksheet.Range(firstCell, lastCell).AsTable());
    }

    public void WriteAsTable<T>(IEnumerable<string> header, IEnumerable<T> data,
        Action<T, Dictionary<string, XLCellValue>> setDic)
    {
        var dataList = data.ToList();
        SetData(dataList, setDic);
        WriteAsTable(header, dataList.Count);
    }

    public void WriteAsTable(IEnumerable<string> header, Action<Dictionary<string, XLCellValue>> setDic)
    {
        SetData(setDic);
        WriteAsTable(header, 1);
    }
}