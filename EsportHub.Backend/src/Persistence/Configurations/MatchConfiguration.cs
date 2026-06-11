using EsportHub.Domain.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EsportHub.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.HasOne(match => match.Team1)
            .WithMany()
            .HasForeignKey(match => match.Team1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(match => match.Team2)
            .WithMany()
            .HasForeignKey(match => match.Team2Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Matches_ScorePairConsistency",
                "(\"Team1Score\" IS NULL AND \"Team2Score\" IS NULL) OR (\"Team1Score\" IS NOT NULL AND \"Team2Score\" IS NOT NULL)");
        });

        builder.HasDiscriminator(match => match.StageType)
            .HasValue<GroupStageMatch>(MatchStageType.Group)
            .HasValue<KnockoutStageMatch>(MatchStageType.Knockout);

        builder.Property(match => match.StageType)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<MatchStageType>(v));
    }
}
