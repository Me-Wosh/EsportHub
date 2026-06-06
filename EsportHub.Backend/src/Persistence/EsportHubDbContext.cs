using EsportHub.Domain;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.Persistence;

public class EsportHubDbContext(DbContextOptions<EsportHubDbContext> options) : DbContext(options)
{
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
