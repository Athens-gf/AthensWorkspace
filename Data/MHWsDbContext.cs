using AthensWorkspace.MHWs.Models;
using AthensWorkspace.Models.MHWs;
using Microsoft.EntityFrameworkCore;

namespace AthensWorkspace.MHWs.Data;

public class MHWsDbContext(DbContextOptions<MHWsDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AmuletSkillGroup>().HasKey(group => new { group.Id, group.SkillId });
        modelBuilder.Entity<AmuletPattern>().HasKey(pattern => new
        {
            pattern.Rare,
            pattern.Group1, pattern.Group2, pattern.Group3,
            pattern.Slot1, pattern.Slot2, pattern.Slot3
        });
    }

    public DbSet<Skill> Skill => Set<Skill>();
    public DbSet<AmuletSkillGroup> AmuletSkillGroup => Set<AmuletSkillGroup>();
    public DbSet<AmuletPattern> AmuletPattern => Set<AmuletPattern>();
    public DbSet<Amulet> Amulet => Set<Amulet>();
}