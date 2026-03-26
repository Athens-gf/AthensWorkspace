using AthensWorkspace.MHWs.Data;
using AthensWorkspace.MHWs.Models;
using AthensWorkspace.Models.MHWs;
using Utility;
using static AthensWorkspace.ViewModels.MHWs.ExAmulet;

namespace AthensWorkspace.ViewModels.MHWs;

public class AmuletVm
{
    // スロットの組み合わせに対応するレア度
    public Dictionary<Slots, string> Slot2RareClass { get; init; }

    // スロットの組み合わせ一覧
    public List<Slots> Slots { get; init; }

    // n番目のスキルに選択できるGroup
    public Dictionary<int, List<byte>> Serial2GroupIds { get; init; }

    // Groupそれぞれに対応するSkillとそのレベル
    public Dictionary<byte, List<(Skill Skill, byte Level)>> GroupId2Skill2Level { get; init; }

    public Dictionary<int, Dictionary<short, string>> Serial2SkillId2GroupIdsClass { get; init; }

    // n番目のスキルに選択できるスキルのリスト
    public Dictionary<int, List<Skill>> Serial2Skill { get; init; }

    // スキルの辞書(種別→アイコン(効果種別)→スキルリスト)
    public Dictionary<SkillType, Dictionary<Icon, List<Skill>>> SkillDic { get; init; }

    public AmuletVm(MHWsDbContext dbContext)
    {
        var basePatterns = dbContext.AmuletPattern.ToList();
        var baseGroups = dbContext.AmuletSkillGroup.ToList();
        var groupDictionary = baseGroups.ToGroupDictionary(group => group.Id);
        var baseSkills = dbContext.Skill.ToList();
        var skillDictionary = baseSkills.ToDictionary(skill => skill.Id);

        var slot2Rares = basePatterns.ToGroupDictionary(pattern => pattern.Slots, pattern => pattern.Rare)
            .SelectValues(list => list.Distinct().OrderBy(t => t).ToList());
        Slot2RareClass = slot2Rares.SelectValues(rares => rares.ToClass(rare => rare.ToString().ToLower()));
        Slots = Slot2RareClass.Keys
            .OrderByDescending(slots => slots.Slot1.IsWeapon())
            .ThenByDescending(slots => slots.Slot1)
            .ThenByDescending(slots => slots.Slot2)
            .ThenByDescending(slots => slots.Slot3).ToList();

        Serial2GroupIds = MakeSerial2GroupIds(basePatterns, groupDictionary, skillDictionary);
        GroupId2Skill2Level = groupDictionary.SelectValues(list => list.Select(group => (skillDictionary[group.SkillId], group.Level)).OrderBy(tuple => tuple.Item1).ToList());

        // すべてのスキルに対応するGroupIds
        var allSkillId2GroupIds = baseGroups.ToGroupDictionary(group => group.SkillId, group => group.Id);
        // n番目のスキルに選択できるスキルに対応するGroupのリスト
        var serial2SkillId2GroupIds = Serial2GroupIds.SelectValues(selectableGroupIds => allSkillId2GroupIds
            // それぞれのグループでフィルタをかける
            .SelectValues(groupIds => groupIds.Intersect(selectableGroupIds).ToList())
            // 対応するグループがないスキルは取り除く
            .Where(pair => pair.Value.Count != 0)
            .ToDictionary()
        );
        Serial2SkillId2GroupIdsClass = serial2SkillId2GroupIds.SelectValues(skillId2GroupIds =>
            skillId2GroupIds.SelectValues(groupIds => groupIds.ToClass(groupId => $"group{groupId}")));

        Serial2Skill = MakeSerialDic(serial => Serial2GroupIds[serial + 1]
            .SelectMany(groupId => groupDictionary[groupId]).Select(group => group.SkillId).Distinct().Select(skillId => skillDictionary[skillId]).OrderBy().ToList());

        SkillDic = baseGroups.Select(group => group.SkillId).Distinct()
            .Select(skillId => skillDictionary[skillId])
            .ToGroupDictionary(skill => skill.Type)
            .SelectValues(list => list.ToGroupDictionary(skill => skill.Icon).SelectValues(skills => skills.OrderBy().ToList()));
    }
}