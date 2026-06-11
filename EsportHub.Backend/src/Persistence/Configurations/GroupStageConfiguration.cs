using EsportHub.Domain.Tournaments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EsportHub.Persistence.Configurations;

public class GroupStageConfiguration : IEntityTypeConfiguration<GroupStage>
{
    public void Configure(EntityTypeBuilder<GroupStage> builder)
    {
        builder.HasMany(groupStage => groupStage.Groups)
            .WithOne(group => group.GroupStage)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
