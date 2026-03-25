using System.Text.Json;
using AthensWorkspace.MHWs.Data;
using AthensWorkspace.MHWs.Models;
using AthensWorkspace.ViewModels.DatabaseFromExcel;
using Microsoft.EntityFrameworkCore;
using Utility.Excel;
using Utility;
using static Utility.Option.ExOption;

namespace AthensWorkspace.MHWs.ViewModels.DatabaseFromExcel;

public class SkillUpVm : UploadItemVm<MHWsDbContext, Skill>
{
    public static string SheetName => "スキル";
    public override string BaseName => "Skills";

    public class DisplayItem : DisplayUpdateItem<Skill>
    {
        public short Id { get; init; }
        public string Name { get; }
        public string Ruby { get; }
        public string SkillType { get; }
        public string Order { get; }
        public string Icon { get; }
        public string MaxLevel { get; }
        public string Explanation { get; }
        public string ExplanationByLevel { get; }

        public DisplayItem((Skill, Skill) item) : base(item)
        {
            Id = ItemDb.Id;
            Name = MakeDisplay(x => x.Name);
            Ruby = MakeDisplay(x => x.Ruby);
            SkillType = MakeDisplay(x => x.Type, type => type.GetText());
            Order = MakeDisplay(x => x.Order);
            Icon = MakeDisplay(x => x.Icon);
            MaxLevel = MakeDisplay(x => x.MaxLevel);
            Explanation = MakeDisplay(x => x.Explanation);
            ExplanationByLevel = MakeDisplay(x => x.ExplanationByLevel);
        }
    }

    private static Dictionary<string, Func<string, object>> MakeConvertExtraDic() =>
        new()
        {
            { "Type", s => s.GetEnumByText<SkillType>().Get },
            { "Icon", s => s.GetEnum<Icon>().Get },
            { "ExplanationByLevel", s => JsonSerializer.Serialize(s.Split("@").Where(e => !e.IsNullOrEmpty()).ToList()) }
        };

    private static List<(string, string)> ConvertProjections =>
    [
        ("名前", "Name"),
        ("読み", "Ruby"),
        ("種別", "Type"),
        ("連番", "Order"),
        ("アイコン", "Icon"),
        ("最大Lv", "MaxLevel"),
        ("説明", "Explanation"),
        ("個別説明", "ExplanationByLevel"),
    ];

    public static List<string> BaseHeader => ConvertProjections.Select(a => a.Item1).ToList();

    public IEnumerable<DisplayItem> UpdateDisplayItems => UpdateDbItems.Zip(UpdatedItems).Select(t => new DisplayItem(t));

    public SkillUpVm()
    {
    }

    public SkillUpVm(MHWsDbContext context, DataMatrix matrix) : base(context, matrix,
        ConvertProjections, Some(MakeConvertExtraDic()),
        x => x.Name, x => x.Name, x => x.Id, (item, obj) => item.Id = (short)obj,
        SheetName, c => c.Skill.ToList())
    {
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