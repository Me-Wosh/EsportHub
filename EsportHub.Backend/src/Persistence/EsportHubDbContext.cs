using EsportHub.Domain;
using EsportHub.Domain.Matches;
using EsportHub.Domain.Teams;
using EsportHub.Domain.Tournaments;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Persistence;

public class EsportHubDbContext(DbContextOptions<EsportHubDbContext> options) : DbContext(options)
{
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<GroupStage> GroupStages => Set<GroupStage>();
    public DbSet<GroupStageMatch> GroupStageMatches => Set<GroupStageMatch>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupTeamStanding> GroupTeamStandings => Set<GroupTeamStanding>();
    public DbSet<KnockoutStage> KnockoutStages => Set<KnockoutStage>();
    public DbSet<KnockoutStageMatch> KnockoutStageMatches => Set<KnockoutStageMatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EsportHubDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimeStamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimeStamps()
    {
        var modifiedEntries = ChangeTracker.Entries<BaseEntity>().Where(e => e.State is EntityState.Modified);

        foreach (var modifiedEntry in modifiedEntries)
        {
            modifiedEntry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
