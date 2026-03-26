using AthensWorkspace.MHWs.Data;
using AthensWorkspace.Models.MHWs;
using AthensWorkspace.ViewModels.DatabaseFromExcel;
using Microsoft.EntityFrameworkCore;
using Utility.Excel;
using static Utility.Option.ExOption;

namespace AthensWorkspace.MHWs.ViewModels.DatabaseFromExcel;

public class AmuletSkillGroupUpVm(MHWsDbContext context, DataMatrix matrix) : UploadItemVm<MHWsDbContext, AmuletSkillGroup>(context, matrix,
    ConvertProjections, Some(MakeConvertExtraDic(context)),
    x => (x.Id, x.SkillId), x => (x.Id, x.SkillId), x => (x.Id, x.SkillId), (_, _) => throw new Exception(),
    SheetName, c => c.AmuletSkillGroup.ToList())
{
    public static string SheetName => "護石スキルグループ";
    public override string BaseName => "AmuletSkillGroups";

    private static List<(string, string)> ConvertProjections =>
    [
        ("グループ", "Id"),
        ("スキル", "SkillId"),
        ("レベル", "Level"),
    ];

    public static List<string> Header => ConvertProjections.Select(a => a.Item1).ToList();

    private static Dictionary<string, Func<string, object>> MakeConvertExtraDic(MHWsDbContext context)
    {
        var dic = context.Skill.ToDictionary(skill => skill.Name, skill => skill.Id);
        return new Dictionary<string, Func<string, object>> { { "SkillId", s => dic[s] } };
    }

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