using AthensWorkspace.MHWs.Data;
using AthensWorkspace.MHWs.Models;
using AthensWorkspace.Models.MHWs;
using AthensWorkspace.ViewModels.DatabaseFromExcel;
using Microsoft.EntityFrameworkCore;
using Utility;
using Utility.Excel;
using static Utility.Option.ExOption;

namespace AthensWorkspace.MHWs.ViewModels.DatabaseFromExcel;

public class AmuletPatternUpVm(MHWsDbContext context, DataMatrix matrix) : UploadItemVm<MHWsDbContext, AmuletPattern>(context, matrix,
    ConvertProjections, Some(MakeConvertExtraDic()),
    x => x, x => x, x => x, (_, _) => throw new Exception(),
    SheetName, c => c.AmuletPattern.ToList())
{
    public static string SheetName => "護石パターン";
    public override string BaseName => "AmuletPatterns";
    private static List<(string, string)> ConvertProjections => Header.Select(h => (h, h)).ToList();
    public static List<string> Header => ["Rare", "Group1", "Group2", "Group3", "Slot1", "Slot2", "Slot3",];

    private static Slot ToSlot(string slot)
    {
        if (slot.IsNullOrEmpty()) return Slot.None;
        var v = 5;
        if (slot.Contains('W'))
        {
            slot = slot.Replace("W", "");
            v = 0;
        }

        return (Slot)(1 << (v + int.Parse(slot) - 1));
    }

    private static Dictionary<string, Func<string, object>> MakeConvertExtraDic() => new()
    {
        { "Rare", s => (Rare)byte.Parse(s) },
        { "Group3", s => byte.TryParse(s, out var v) ? v : 0 },
        { "Slot1", s => ToSlot(s) },
        { "Slot2", s => ToSlot(s) },
        { "Slot3", s => ToSlot(s) },
    };

    public override void AddItems(DbContext context)
    {
        context.AddRange(AddedItems);
        context.SaveChanges();
    }

    public override void UpdateItems(DbContext context)
    {
        context.UpdateRange(UpdatedItems);
        context.SaveChanges();
    }
}