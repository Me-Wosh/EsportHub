using EsportHub.Domain.Matches;
using EsportHub.Domain.Teams;
using EsportHub.Domain.Tournaments;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EsportHub.UnitTests.Persistence;

public class EfConfigurationTests : IDisposable
{
    private readonly EsportHubDbContext _context;

    public EfConfigurationTests()
    {
        var options = new DbContextOptionsBuilder<EsportHubDbContext>()
            .UseInMemoryDatabase(nameof(EfConfigurationTests))
            .Options;
        _context = new EsportHubDbContext(options);
    }

    public void Dispose() => _context.Dispose();

    // TeamConfiguration

    [Fact]
    public void TeamName_IsRequired()
    {
        var property = GetProperty<Team>(nameof(Team.Name));
        Assert.False(property.IsNullable);
    }

    [Fact]
    public void TeamName_HasMaxLength100()
    {
        var property = GetProperty<Team>(nameof(Team.Name));
        Assert.Equal(TeamConstraints.NameMaxLength, property.GetMaxLength());
    }

    // PlayerConfiguration

    [Fact]
    public void PlayerName_IsRequired()
    {
        var property = GetProperty<Player>(nameof(Player.Name));
        Assert.False(property.IsNullable);
    }

    [Fact]
    public void PlayerName_HasMaxLength100()
    {
        var property = GetProperty<Player>(nameof(Player.Name));
        Assert.Equal(PlayerConstraints.NameMaxLength, property.GetMaxLength());
    }

    // TournamentConfiguration

    [Fact]
    public void TournamentName_IsRequired()
    {
        var property = GetProperty<Tournament>(nameof(Tournament.Name));
        Assert.False(property.IsNullable);
    }

    [Fact]
    public void TournamentName_HasMaxLength100()
    {
        var property = GetProperty<Tournament>(nameof(Tournament.Name));
        Assert.Equal(TournamentConstraints.NameMaxLength, property.GetMaxLength());
    }

    [Fact]
    public void TournamentStatus_HasValueConverter()
    {
        var property = GetProperty<Tournament>(nameof(Tournament.Status));
        Assert.NotNull(property.GetValueConverter());
    }

    [Fact]
    public void GroupStage_TournamentForeignKey_HasDeleteBehaviorRestrict()
    {
        var fk = GetUniqueForeignKey<GroupStage, Tournament>();
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void KnockoutStage_TournamentForeignKey_HasDeleteBehaviorRestrict()
    {
        var fk = GetUniqueForeignKey<KnockoutStage, Tournament>();
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void Team_TournamentForeignKey_HasDeleteBehaviorRestrict()
    {
        var fk = GetForeignKeys<Team, Tournament>().Single();
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    // GroupConfiguration

    [Fact]
    public void GroupName_IsRequired()
    {
        var property = GetProperty<Group>(nameof(Group.Name));
        Assert.False(property.IsNullable);
    }

    [Fact]
    public void GroupName_HasMaxLength20()
    {
        var property = GetProperty<Group>(nameof(Group.Name));
        Assert.Equal(GroupConstraints.NameMaxLength, property.GetMaxLength());
    }

    [Fact]
    public void Team_GroupForeignKey_HasDeleteBehaviorRestrict()
    {
        var fk = GetForeignKeys<Team, Group>().Single();
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void GroupStageMatch_GroupForeignKey_HasDeleteBehaviorRestrict()
    {
        var fk = GetUniqueForeignKey<GroupStageMatch, Group>();
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void GroupTeamStanding_GroupForeignKey_HasDeleteBehaviorRestrict()
    {
        var fk = GetForeignKeys<GroupTeamStanding, Group>().Single();
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void Group_GroupStageForeignKey_HasDeleteBehaviorRestrict()
    {
        var fk = GetUniqueForeignKey<Group, GroupStage>();
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    // MatchConfiguration

    [Fact]
    public void MatchTeam1Score_IsNullable()
    {
        var property = GetProperty<Match>(nameof(Match.Team1Score));
        Assert.True(property.IsNullable);
    }

    [Fact]
    public void MatchTeam2Score_IsNullable()
    {
        var property = GetProperty<Match>(nameof(Match.Team2Score));
        Assert.True(property.IsNullable);
    }

    [Fact]
    public void MatchStageType_HasValueConverter()
    {
        var property = GetProperty<Match>(nameof(Match.StageType));
        Assert.NotNull(property.GetValueConverter());
    }

    [Fact]
    public void Match_HasDiscriminatorOnStageTypeProperty()
    {
        var entityType = _context.Model.FindEntityType(typeof(Match))!;
        var discriminatorProperty = entityType.FindDiscriminatorProperty();
        Assert.NotNull(discriminatorProperty);
        Assert.Equal(nameof(Match.StageType), discriminatorProperty.Name);
    }

    [Fact]
    public void Match_Team1ForeignKey_HasDeleteBehaviorRestrict()
    {
        var entityType = _context.Model.FindEntityType(typeof(Match))!;
        var fk = entityType.GetForeignKeys()
            .Where(fk => fk.PrincipalEntityType.ClrType == typeof(Team))
            .Single(fk => fk.Properties.Any(p => p.Name == nameof(Match.Team1Id)));
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void Match_Team2ForeignKey_HasDeleteBehaviorRestrict()
    {
        var entityType = _context.Model.FindEntityType(typeof(Match))!;
        var fk = entityType.GetForeignKeys()
            .Where(fk => fk.PrincipalEntityType.ClrType == typeof(Team))
            .Single(fk => fk.Properties.Any(p => p.Name == nameof(Match.Team2Id)));
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    // GroupTeamStandingConfiguration

    [Fact]
    public void GroupTeamStanding_HasUniqueIndexOnGroupIdAndTeamId()
    {
        var entityType = _context.Model.FindEntityType(typeof(GroupTeamStanding))!;
        var index = entityType.GetIndexes().Single(i =>
            i.Properties.Any(p => p.Name == nameof(GroupTeamStanding.GroupId)) &&
            i.Properties.Any(p => p.Name == nameof(GroupTeamStanding.TeamId)));
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void GroupTeamStanding_TeamForeignKey_HasDeleteBehaviorRestrict()
    {
        var fk = GetUniqueForeignKey<GroupTeamStanding, Team>();
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    // KnockoutStageMatchConfiguration

    [Fact]
    public void KnockoutStageMatchSide_HasValueConverter()
    {
        var property = GetProperty<KnockoutStageMatch>(nameof(KnockoutStageMatch.Side));
        Assert.NotNull(property.GetValueConverter());
    }

    [Fact]
    public void KnockoutStageMatchRound_HasValueConverter()
    {
        var property = GetProperty<KnockoutStageMatch>(nameof(KnockoutStageMatch.Round));
        Assert.NotNull(property.GetValueConverter());
    }

    // KnockoutStageConfiguration

    [Fact]
    public void KnockoutStageMatch_KnockoutStageForeignKey_HasDeleteBehaviorRestrict()
    {
        var fk = GetUniqueForeignKey<KnockoutStageMatch, KnockoutStage>();
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    private IReadOnlyProperty GetProperty<TEntity>(string propertyName) where TEntity : class =>
        _context.Model.FindEntityType(typeof(TEntity))!.FindProperty(propertyName)!;

    private IReadOnlyForeignKey GetUniqueForeignKey<TDependent, TPrincipal>()
        where TDependent : class
        where TPrincipal : class =>
        _context.Model.FindEntityType(typeof(TDependent))!
            .GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(TPrincipal));

    private IEnumerable<IReadOnlyForeignKey> GetForeignKeys<TDependent, TPrincipal>()
        where TDependent : class
        where TPrincipal : class =>
        _context.Model.FindEntityType(typeof(TDependent))!
            .GetForeignKeys()
            .Where(fk => fk.PrincipalEntityType.ClrType == typeof(TPrincipal));
}
