using EsportHub.Domain.Tournaments;
using EsportHub.Features.Tournaments;

namespace EsportHub.UnitTests.Features.Tournaments;

public class UpdateGroupNameCommandHandlerTests : HandlerTestBase
{
    private readonly UpdateGroupNameCommandHandler _handler;

    public UpdateGroupNameCommandHandlerTests()
    {
        _handler = new UpdateGroupNameCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_GivenNonExistentGroupId_ReturnsNotFound()
    {
        var result = await _handler.Handle(
            new UpdateGroupNameCommand(Guid.NewGuid(), Guid.NewGuid(), "New Name"),
            CancellationToken.None);

        Assert.True(result.IsNotFound());
    }

    [Fact]
    public async Task Handle_GivenGroupStageIsClosed_ReturnsInvalid()
    {
        var (tournament, groupStage) = await SeedClosedGroupStageAsync();
        var group = groupStage.Groups.First();

        var result = await _handler.Handle(
            new UpdateGroupNameCommand(tournament.Id, group.Id, "New Name"),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenValidName_ReturnsUpdatedGroupName()
    {
        var (tournament, groupStage) = await SeedGroupStageAsync();
        var group = groupStage.Groups.First();

        var result = await _handler.Handle(
            new UpdateGroupNameCommand(tournament.Id, group.Id, "Pool A"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Pool A", result.Value.Name);
    }

    [Fact]
    public async Task Handle_GivenValidName_PersistsUpdatedGroupName()
    {
        var (tournament, groupStage) = await SeedGroupStageAsync();
        var group = groupStage.Groups.First();

        await _handler.Handle(
            new UpdateGroupNameCommand(tournament.Id, group.Id, "Pool A"),
            CancellationToken.None);

        var getHandler = new GetGroupQueryHandler(Context);
        var queryResult = await getHandler.Handle(
            new GetGroupQuery(tournament.Id, group.Id),
            CancellationToken.None);

        Assert.True(queryResult.IsSuccess);
        Assert.Equal("Pool A", queryResult.Value.Name);
    }

    [Fact]
    public async Task Handle_GivenEmptyName_ReturnsInvalid()
    {
        var (tournament, groupStage) = await SeedGroupStageAsync();
        var group = groupStage.Groups.First();

        var result = await _handler.Handle(
            new UpdateGroupNameCommand(tournament.Id, group.Id, ""),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }

    [Fact]
    public async Task Handle_GivenNameExceedingMaxLength_ReturnsInvalid()
    {
        var (tournament, groupStage) = await SeedGroupStageAsync();
        var group = groupStage.Groups.First();
        var longName = new string('A', GroupConstraints.NameMaxLength + 1);

        var result = await _handler.Handle(
            new UpdateGroupNameCommand(tournament.Id, group.Id, longName),
            CancellationToken.None);

        Assert.True(result.IsInvalid());
    }
}
