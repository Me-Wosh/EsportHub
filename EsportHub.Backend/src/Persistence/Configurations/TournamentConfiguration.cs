using EsportHub.Domain.Teams;
using EsportHub.Domain.Tournaments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EsportHub.Persistence.Configurations;

public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.Property(tournament => tournament.Name)
            .HasMaxLength(TournamentConstraints.NameMaxLength);

        builder.Property(tournament => tournament.Status)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<TournamentStatus>(v));

        builder.HasOne(tournament => tournament.GroupStage)
            .WithOne(groupStage => groupStage.Tournament)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tournament => tournament.KnockoutStage)
            .WithOne(knockoutStage => knockoutStage.Tournament)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany<Team>()
            .WithOne(team => team.Tournament)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
