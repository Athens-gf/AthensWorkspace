using System.ComponentModel.DataAnnotations;

namespace AthensWorkspace.Models.MHWs;

public class AmuletSkillGroup
{
    [Display(Name = "グループ")] public byte Id { get; set; }
    [Display(Name = "スキル")] public short SkillId { get; set; }
    public byte Level { get; set; }

    public override bool Equals(object? obj) => Equals(obj as AmuletSkillGroup);
    private bool Equals(AmuletSkillGroup? other) => other != null && Id == other.Id && SkillId == other.SkillId && Level == other.Level;

    public override int GetHashCode() => HashCode.Combine(Id, SkillId, Level);
    public static bool operator ==(AmuletSkillGroup? left, AmuletSkillGroup? right) => Equals(left, right);
    public static bool operator !=(AmuletSkillGroup? left, AmuletSkillGroup? right) => !Equals(left, right);
}