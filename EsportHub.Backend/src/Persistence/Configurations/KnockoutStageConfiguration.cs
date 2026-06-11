using EsportHub.Domain.Tournaments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EsportHub.Persistence.Configurations;

public class KnockoutStageConfiguration : IEntityTypeConfiguration<KnockoutStage>
{
    public void Configure(EntityTypeBuilder<KnockoutStage> builder)
    {
        builder.HasMany(knockoutStage => knockoutStage.Matches)
            .WithOne(match => match.KnockoutStage)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
