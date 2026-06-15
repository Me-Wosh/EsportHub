using EsportHub.Domain.Tournaments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EsportHub.Persistence.Configurations;

public class GroupTeamStandingConfiguration : IEntityTypeConfiguration<GroupTeamStanding>
{
    public void Configure(EntityTypeBuilder<GroupTeamStanding> builder)
    {
        builder.HasIndex(standing => new { standing.GroupId, standing.TeamId })
            .IsUnique();

        // can't have a unique index on GroupId and Position as it will create a circular dependency when trying to move
        // a team up or down in the standings, as for exmaple changing position 1 to position 2 requires position 2 to
        // be free already

        builder.HasOne(standing => standing.Team)
            .WithOne()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
