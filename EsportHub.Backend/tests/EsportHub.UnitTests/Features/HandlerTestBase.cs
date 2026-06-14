using EsportHub.Domain.Teams;
using EsportHub.Domain.Tournaments;
using EsportHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.UnitTests.Features;

public abstract class HandlerTestBase : IDisposable
{
    protected readonly EsportHubDbContext Context;

    protected HandlerTestBase()
    {
        var options = new DbContextOptionsBuilder<EsportHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Context = new EsportHubDbContext(options);
    }

    public void Dispose() => Context.Dispose();

    protected async Task<Tournament> SeedTournamentAsync(string name = "ESL Pro League")
    {
        var tournament = Tournament.Create(name).Value;
        Context.Tournaments.Add(tournament);
        await Context.SaveChangesAsync();
        return tournament;
    }

    protected async Task<Team> SeedTeamAsync(Guid tournamentId, string name = "Team Alpha")
    {
        var team = Team.Create(name, tournamentId).Value;
        Context.Teams.Add(team);
        await Context.SaveChangesAsync();
        return team;
    }

    protected async Task<(Tournament Tournament, GroupStage GroupStage)> SeedGroupStageAsync()
    {
        var tournament = Tournament.Create("ESL Pro League").Value;

        var teams = Enumerable.Range(1, TournamentConstraints.TeamsRequiredCount)
            .Select(i =>
            {
                var team = Team.Create($"Team {i}", tournament.Id).Value;
                for (var j = 1; j <= TeamConstraints.PlayersMinCount; j++)
                    team.AddPlayer($"T{i} Player {j}");
                return team;
            })
            .ToList();

        var groupNames = Enumerable.Range(0, TournamentConstraints.GroupsRequiredCount)
            .Select(i => $"Group {(char)('A' + i)}")
            .ToList();

        // Build the full graph in memory before tracking, so EF inserts everything
        // as new (Added) without needing to UPDATE already-tracked team rows
        tournament.Start(teams, groupNames);

        Context.Tournaments.Add(tournament);
        await Context.SaveChangesAsync();

        return (tournament, tournament.GroupStage!);
    }

    protected async Task<(Tournament Tournament, GroupStage GroupStage)> SeedClosedGroupStageAsync()
    {
        var (tournament, _) = await SeedGroupStageAsync();

        var trackedGroupStage = await Context.GroupStages.SingleAsync(gs => gs.TournamentId == tournament.Id);
        typeof(GroupStage)
            .GetProperty(nameof(GroupStage.IsClosed))!
            .SetValue(trackedGroupStage, true);
        await Context.SaveChangesAsync();

        return (tournament, trackedGroupStage);
    }
}
