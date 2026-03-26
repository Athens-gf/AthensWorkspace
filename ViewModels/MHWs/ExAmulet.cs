using AthensWorkspace.MHWs.Data;
using AthensWorkspace.MHWs.Models;
using AthensWorkspace.Models.MHWs;
using Microsoft.AspNetCore.Mvc;
using Utility;

// ReSharper disable AccessToModifiedClosure

namespace AthensWorkspace.ViewModels.MHWs;

public static class ExAmulet
{
    public static Dictionary<int, T> MakeSerialDic<T>(Func<int, T> elementSelector) => Enumerable.Range(0, 3).ToDictionary(serial => serial + 1, elementSelector);
    public static string ToClass<T>(this IEnumerable<T> source, Func<T, string> toClass) => source.Select(toClass).Join(" ");

    private static (List<AmuletPattern>, List<byte>) FilterGroupIds(
        List<AmuletPattern> usablePatterns,
        List<List<byte>> decidedGroupIds,
        List<List<byte>> decidedGroupIdPairs
    )
    {
        var filteredPatterns = usablePatterns
            .Where(pattern => decidedGroupIds.All(groupIds => pattern.GroupIds.Any(groupIds.Contains)))
            .ToList();

        var allGroupIds = filteredPatterns
            .Select(pattern => pattern.GroupIds.Where(groupId => groupId != 0).OrderBy().ToArray())
            .DistinctBy(groupIds => groupIds.Join("-"))
            .SelectMany(baseGroupIds => decidedGroupIdPairs.Count == 0
                ? baseGroupIds
                : decidedGroupIdPairs.SelectMany(removingGroupIds =>
                {
                    if (!removingGroupIds.All(baseGroupIds.Contains)) return [];
                    var groupIds = baseGroupIds.ToList();
                    removingGroupIds.ForEach(groupId => groupIds.Remove(groupId));
                    return groupIds;
                })).ToOrderedList();

        return (filteredPatterns, allGroupIds);
    }

    public static Dictionary<int, List<byte>> MakeSerial2GroupIds(
        List<AmuletPattern> patterns,
        Dictionary<byte, List<AmuletSkillGroup>> groupDictionary,
        Dictionary<short, Skill> skillDictionary
    )
    {
        var dictionary = new Dictionary<int, List<byte>>();
        var groupId2SkillIds = groupDictionary.SelectValues(list => list.Select(group => group.SkillId).ToOrderedList());
        var groupId2Skills = groupId2SkillIds.SelectValues(list => list.Select(skillId => skillDictionary[skillId]).ToList());
        // 第一グループは武器系のスキルが含まれるグループのみ
        var groupIds1 = groupId2Skills.Where(pair => pair.Value.Any(skill => skill.Type == SkillType.T0Weapon)).ToOrderedList(pair => pair.Key);
        // 第二グループは第一グループのスキルが含まれた時に可能なグループ群
        (patterns, var groupIds2) = FilterGroupIds(patterns, [groupIds1],
            groupIds1.Select(groupId => new List<byte> { groupId }).ToList());
        // 第三グループは第一グループと第二グループのスキルが両方含まれた時に可能なグループ群
        var (_, groupIds3) = FilterGroupIds(patterns, [groupIds1, groupIds2],
            groupIds1.SelectMany(groupId1 => groupIds2.Select(groupId2 => new List<byte> { groupId1, groupId2 })).ToList());
        dictionary[1] = groupIds1;
        dictionary[2] = groupIds2;
        dictionary[3] = groupIds3;
        return dictionary;
    }

    private static List<Slots> CalcSelectableSlots(List<AmuletPattern> basePatterns, Rare? rare) =>
        basePatterns.Where(pattern => rare == null || pattern.Rare == rare).Select(pattern => pattern.Slots).Distinct().ToList();

    private static Rare? ToRare(byte? rawRare) => rawRare != null ? (Rare)rawRare : null;

    private static Slots? ToSlots(string? rawSlots, List<AmuletPattern> usablePatterns)
    {
        var slotsDic = usablePatterns.Select(pattern => pattern.Slots).Distinct().ToDictionary(ss => ss.ValStr);
        return rawSlots != null && slotsDic.TryGetValue(rawSlots, out var slots) ? slots : null;
    }

    public static (List<AmuletPattern>, Rare?, Slots?) ToRareSlots(MHWsDbContext dbContext, byte? rawRare, string? rawSlots)
    {
        var rare = ToRare(rawRare);
        var patterns = dbContext.AmuletPattern.Where(pattern => rare == null || pattern.Rare == rare).ToList();
        var slots = ToSlots(rawSlots, patterns);
        var rares = patterns.Where(pattern => pattern.Slots == slots).Select(pattern => pattern.Rare).Distinct().ToList();
        if (rares.Count == 1) rare = rares[0];
        return (patterns, rare, slots);
    }

    public static List<(short, byte?, int)> ToSkills(
        short? skillId1, byte? level1,
        short? skillId2, byte? level2,
        short? skillId3, byte? level3
    ) => new List<(short? skillId, byte? level)> { (skillId1, level1), (skillId2, level2), (skillId3, level3) }
        .ZipWithIndexBase1()
        .Where(tuple => tuple.value.skillId != null)
        .Select(tuple => ((short)tuple.value.skillId!, tuple.value.level, tuple.index)).ToList();

    private static List<List<byte>> CalcDecidedGroupIdPairs(int count, Dictionary<int, List<byte>> predictedGroupIdsDic)
    {
        switch (count)
        {
            case 0:
                return [];
            case 1:
            {
                var (_, decidedGroupIdsA) = predictedGroupIdsDic.First();
                return decidedGroupIdsA.Select(groupId => new List<byte> { groupId }).ToList();
            }
            case 2:
            {
                var (_, decidedGroupIdsA) = predictedGroupIdsDic.First();
                var (_, decidedGroupIdsB) = predictedGroupIdsDic.Skip(1).First();
                return decidedGroupIdsA.SelectMany(groupIdA => decidedGroupIdsB.Select(groupIdB => new List<byte> { groupIdA, groupIdB })).ToList();
            }
            default:
                throw new NotImplementedException();
        }
    }

    private class GuessValue(
        List<AmuletPattern> useablePatterns,
        List<AmuletPattern> guessPatterns,
        List<AmuletSkillGroup> usableGroups,
        Dictionary<int, List<byte>> predictedGroupIdsDic,
        Dictionary<int, List<byte>> selectableGroupIdsDic
    )
    {
        public List<AmuletPattern> UseablePatterns { get; } = useablePatterns;
        public List<AmuletSkillGroup> UsableGroups { get; } = usableGroups;
        public List<AmuletPattern> GuessPatterns { get; } = guessPatterns;
        public Dictionary<int, List<byte>> PredictedGroupIdsDic { get; } = predictedGroupIdsDic;
        public Dictionary<int, List<byte>> SelectableGroupIdsDic { get; } = selectableGroupIdsDic;

        public List<List<byte>> GetDecidedGroupIdPairs() => CalcDecidedGroupIdPairs(PredictedGroupIdsDic.Count, PredictedGroupIdsDic);
    }

    private static GuessValue Guess(
        List<AmuletPattern> basePatterns,
        List<AmuletSkillGroup> baseGroups,
        List<Skill> baseSkills,
        Rare? rare, Slots? slots, IEnumerable<(short skillId, byte? level, int index)> skills
    )
    {
        // レア度とスロット指定から使用できるパターンを抽出
        var usablePatterns = basePatterns.Where(pattern => (rare == null || rare == pattern.Rare) && (slots == null || slots == pattern.Slots)).ToList();
        // 使用できるパターンから使用できるグループIdを抽出
        var usableGroupIds = usablePatterns.SelectMany(pattern => pattern.GroupIds).ToOrderedList(groupId => groupId != 0);
        // 使用できるグループIdから使用できるグループ情報を抽出
        var usableGroups = baseGroups.Where(group => usableGroupIds.Contains(group.Id)).ToList();
        // スキルIdとレベルから対応するグループIdを取得する辞書を作成
        var skillIdWithLevel2GroupIds = usableGroups.ToGroupDictionary(group => (group.SkillId, group.Level), group => group.Id);
        // 入力が確定しているスキル(=levelがnullでないもの)の「スキル順→推測されるグループId」の辞書を作成
        var predictedGroupIdsDic = skills.Where(tuple => tuple is { level: not null })
            .Select(tuple => (tuple.skillId, level: (byte)tuple.level!, tuple.index))
            .Select(tuple => (tuple.index, skillIdWithLevel2GroupIds.TryGetValue((tuple.skillId, tuple.level), out var groupIds) ? groupIds : []))
            .ToDictionary();

        // 確定済みのスキルのグループIdのリストのリスト[スキル1個目のグループIdのリスト, スキル2個目のグループIdのリスト]
        var decidedGroupIds = predictedGroupIdsDic.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList();

        var guessPatterns = usablePatterns
            .Where(pattern => decidedGroupIds.All(groupIds => pattern.GroupIds.Any(groupIds.Contains)))
            .ToList();

        var groupIdsFromSerial = MakeSerial2GroupIds(
            usablePatterns,
            usableGroups.ToGroupDictionary(group => group.Id),
            baseSkills.ToDictionary(skill => skill.Id)
        );
        return new GuessValue(usablePatterns, guessPatterns, usableGroups, predictedGroupIdsDic, groupIdsFromSerial);
    }

    private static List<byte> GuessGroupIds(
        List<AmuletPattern> guessPatterns,
        List<List<byte>> decidedGroupIdPairs,
        List<byte> groupIdsFromSerial
    )
    {
        return guessPatterns
            .Select(pattern => pattern.GroupIds.Where(groupId => groupId != 0).OrderBy().ToArray())
            .DistinctBy(groupIds => groupIds.Join("-"))
            .SelectMany(baseGroupIds => decidedGroupIdPairs.Count == 0
                ? baseGroupIds
                : decidedGroupIdPairs.SelectMany(removingGroupIds =>
                {
                    if (!removingGroupIds.All(baseGroupIds.Contains)) return [];
                    var groupIds = baseGroupIds.ToList();
                    removingGroupIds.ForEach(groupId => groupIds.Remove(groupId));
                    return groupIds;
                }))
            .ToOrderedList()
            .Intersect(groupIdsFromSerial)
            .ToList();
    }

    public static List<byte> GuessLevels(
        List<AmuletPattern> basePatterns,
        List<AmuletSkillGroup> baseGroups,
        List<Skill> baseSkills,
        Rare? rare, Slots? slots, List<(short skillId, byte? level, int index)> skills
    )
    {
        // レベルの候補を受け取りたいスキル(=levelがnullのもの)のIdを取得
        var (selectedSkillId, _, serial) = skills.First(tuple => tuple is { level: null });

        var guess = Guess(basePatterns, baseGroups, baseSkills, rare, slots, skills.Where(tuple => tuple.index <= serial));
        var usableGroups = guess.UsableGroups;
        var guessPatterns = guess.GuessPatterns;
        var decidedGroupIdPairs = guess.GetDecidedGroupIdPairs();
        var groupIdsFromSerial = guess.SelectableGroupIdsDic[serial];

        var guessGroupIds = GuessGroupIds(guessPatterns, decidedGroupIdPairs, groupIdsFromSerial);
        var guessGroups = usableGroups.Where(group => guessGroupIds.Contains(group.Id)).ToList();

        var skillId2Groups = guessGroups.ToGroupDictionary(group => group.SkillId);
        List<byte> selectableLevels = [];
        if (skillId2Groups.TryGetValue(selectedSkillId, out var groups))
            selectableLevels = groups.ToOrderedList(group => group.Level);

        return selectableLevels;
    }

    public static JsonResult JudgeAmulet(
        List<AmuletPattern> basePatterns,
        List<AmuletSkillGroup> baseGroups,
        List<Skill> baseSkills,
        Rare? rare, Slots? slots,
        List<(short skillId, byte? level, int index)> skills
    )
    {
        var guess = Guess(basePatterns, baseGroups, baseSkills, rare, slots, skills);
        var useablePatterns = guess.UseablePatterns;
        var guessPatterns = guess.GuessPatterns;
        var predictedGroupIdsDic = guess.PredictedGroupIdsDic;
        var selectableGroupIdsDic = guess.SelectableGroupIdsDic;

        foreach (var (serial, groupIdsFromSerial) in selectableGroupIdsDic)
        {
            var decidedGroupIdPairs = CalcDecidedGroupIdPairs(Math.Min(serial - 1, predictedGroupIdsDic.Count), predictedGroupIdsDic);
            var guessGroupIds = GuessGroupIds(useablePatterns, decidedGroupIdPairs, groupIdsFromSerial);
            selectableGroupIdsDic[serial] = guessGroupIds;
        }

        if (rare == null && guessPatterns.Select(pattern => pattern.Rare).Distinct().ToList() is [var r])
            rare = r;
        var selectableSlots = CalcSelectableSlots(basePatterns, rare).Select(s => s.Id);
        var resetSkillSerials = predictedGroupIdsDic
            .Where(pair => pair.Value.Count == 0 || !pair.Value.Intersect(selectableGroupIdsDic[pair.Key]).Any())
            .Select(pair => pair.Key).ToList();
        foreach (var serial in resetSkillSerials) predictedGroupIdsDic[serial] = [];

        return new JsonResult(new
        {
            rare, slots = slots?.Id,
            selectableSlots,
            predictedGroupIdsDic,
            selectableGroupIdsDic,
            resetSkillSerials
        });
    }
}