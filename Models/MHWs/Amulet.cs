using System.ComponentModel.DataAnnotations;
using AthensWorkspace.MHWs.Models;

namespace AthensWorkspace.Models.MHWs;

public class Amulet
{
    [Key] public int Id { get; set; }
    public int UserId { get; set; }
    public byte Rare { get; set; }
    public short SkillId1 { get; set; }
    public byte Level1 { get; set; }
    public short SkillId2 { get; set; }
    public byte Level2 { get; set; }
    public short? SkillId3 { get; set; }
    public byte? Level3 { get; set; }
    public Slot Slot1 { get; set; }
    public Slot Slot2 { get; set; }
    public Slot Slot3 { get; set; }
}