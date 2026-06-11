using EsportHub.Domain.Tournaments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EsportHub.Persistence.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.Property(group => group.Name)
            .HasMaxLength(GroupConstraints.NameMaxLength);

        builder.HasMany(group => group.Teams)
            .WithOne()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(group => group.Matches)
            .WithOne(match => match.Group)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(group => group.Standings)
            .WithOne(standing => standing.Group)
            .OnDelete(DeleteBehavior.Restrict);
    }
}