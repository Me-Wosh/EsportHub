using EsportHub.Domain.Matches;
using EsportHub.Domain.Tournaments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EsportHub.Persistence.Configurations;

public class KnockoutStageMatchConfiguration : IEntityTypeConfiguration<KnockoutStageMatch>
{
    public void Configure(EntityTypeBuilder<KnockoutStageMatch> builder)
    {
        builder.Property(match => match.Side)
            .HasConversion(
                v => v == null ? null : v.ToString(),
                v => v == null ? null : Enum.Parse<KnockoutStageSide>(v));

        builder.Property(match => match.Round)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<KnockoutStageRound>(v));
    }
}
