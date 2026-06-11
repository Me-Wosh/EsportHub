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

        builder.HasIndex(standing => new { standing.GroupId, standing.Position })
            .IsUnique();

        builder.HasOne(standing => standing.Team)
            .WithOne()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
