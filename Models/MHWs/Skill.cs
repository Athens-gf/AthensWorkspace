using System.ComponentModel.DataAnnotations;
using Utility;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace AthensWorkspace.MHWs.Models;

public class Skill : IComparable<Skill>
{
    [Key] public short Id { get; set; }
    [Display(Name = "名前")] [MaxLength(32)] public string Name { get; set; } = null!;
    [Display(Name = "読み")] [MaxLength(32)] public string Ruby { get; set; } = null!;
    [Display(Name = "種別")] public SkillType Type { get; set; }
    [Display(Name = "連番")] public short Order { get; set; }
    [Display(Name = "アイコン")] public Icon Icon { get; set; }
    [Display(Name = "最大Lv")] public byte MaxLevel { get; set; }
    [Display(Name = "説明"), MaxLength(128)] public string Explanation { get; set; } = null!;

    [Display(Name = "個別説明"), MaxLength(2048)]
    public string ExplanationByLevel { get; set; } = null!;

    public override bool Equals(object? obj) => Equals(obj as Skill);

    protected bool Equals(Skill? other) =>
        other != null && Name == other.Name && Ruby == other.Ruby && Type == other.Type && Order == other.Order &&
        Icon == other.Icon && MaxLevel == other.MaxLevel &&
        Explanation == other.Explanation && ExplanationByLevel == other.ExplanationByLevel;

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Id);
        hashCode.Add(Name);
        hashCode.Add(Ruby);
        hashCode.Add((int)Type);
        hashCode.Add(Order);
        hashCode.Add((int)Icon);
        hashCode.Add(MaxLevel);
        hashCode.Add(Explanation);
        hashCode.Add(ExplanationByLevel);
        return hashCode.ToHashCode();
    }

    public static bool operator ==(Skill? left, Skill? right) => Equals(left, right);
    public static bool operator !=(Skill? left, Skill? right) => !Equals(left, right);

    public int CompareTo(Skill? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        var typeComparison = Type.CompareTo(other.Type);
        if (typeComparison != 0) return typeComparison;
        var iconComparison = Icon.CompareTo(other.Icon);
        return iconComparison != 0 ? iconComparison : Order.CompareTo(other.Order);
    }
}

public static class ExSkill
{
    public static List<string> ExplanationByLevel(this Skill skill) =>
        skill.ExplanationByLevel.TryDeserialize<List<string>>(out var items) ? items : [];
}