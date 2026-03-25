using AthensWorkspace.MHWs.Models;
using Microsoft.EntityFrameworkCore;

namespace AthensWorkspace.MHWs.Data;

public class MHWsDbContext(DbContextOptions<MHWsDbContext> options) : DbContext(options)
{
    public DbSet<Skill> Skill => Set<Skill>();
}