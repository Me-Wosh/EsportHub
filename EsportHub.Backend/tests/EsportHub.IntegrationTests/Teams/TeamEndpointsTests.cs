using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using EsportHub.Domain.Teams;
using EsportHub.Domain.Tournaments;
using EsportHub.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EsportHub.IntegrationTests.Teams;

public class TeamEndpointsTests : IClassFixture<EsportHubWebApplicationFactory>, IAsyncLifetime
{
    private readonly EsportHubWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TeamEndpointsTests(EsportHubWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Tournament> SeedTournamentAsync(string name = "ESL Pro League")
    {
        return await _factory.ExecuteDbContextAsync(async db =>
        {
            var tournament = Tournament.Create(name).Value;
            db.Tournaments.Add(tournament);
            await db.SaveChangesAsync();
            return tournament;
        });
    }

    private async Task<Team> SeedTeamAsync(Guid tournamentId, string name = "Team Alpha")
    {
        return await _factory.ExecuteDbContextAsync(async db =>
        {
            var team = Team.Create(name, tournamentId).Value;
            db.Teams.Add(team);
            await db.SaveChangesAsync();
            return team;
        });
    }

    private async Task SetTournamentStatusAsync(Guid tournamentId, TournamentStatus status)
    {
        await _factory.ExecuteDbContextAsync(async db =>
        {
            await db.Database.ExecuteSqlAsync(
                $@"UPDATE ""Tournaments"" SET ""Status"" = {(int)status} WHERE ""Id"" = {tournamentId}");
        });
    }

    private static async Task<JsonNode?> GetResponseValueAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(content)?["value"];
    }

    // GET /api/teams

    [Fact]
    public async Task GetTeams_ReturnsEmptyList_WhenNoTeamsExist()
    {
        var response = await _client.GetAsync("/api/teams");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await GetResponseValueAsync(response);
        Assert.Empty(value!.AsArray());
    }

    [Fact]
    public async Task GetTeams_ReturnsAllTeams_WhenTeamsExist()
    {
        var tournament = await SeedTournamentAsync();
        await SeedTeamAsync(tournament.Id, "Team Alpha");
        await SeedTeamAsync(tournament.Id, "Team Bravo");

        var response = await _client.GetAsync("/api/teams");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await GetResponseValueAsync(response);
        Assert.Equal(2, value!.AsArray().Count);
    }

    [Fact]
    public async Task GetTeams_ReturnsFilteredTeams_ByTournamentId()
    {
        var tournament1 = await SeedTournamentAsync("Tournament 1");
        var tournament2 = await SeedTournamentAsync("Tournament 2");
        await SeedTeamAsync(tournament1.Id, "Team Alpha");
        await SeedTeamAsync(tournament2.Id, "Team Bravo");

        var response = await _client.GetAsync($"/api/teams?tournamentId={tournament1.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await GetResponseValueAsync(response);
        var team = Assert.Single(value!.AsArray());
        Assert.Equal("Team Alpha", team?["name"]?.GetValue<string>());
    }

    [Fact]
    public async Task GetTeams_ReturnsNotFound_WhenTournamentDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/teams?tournamentId={Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // GET /api/teams/{id}

    [Fact]
    public async Task GetTeam_ReturnsTeam_WhenTeamExists()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id, "Team Alpha");

        var response = await _client.GetAsync($"/api/teams/{team.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await GetResponseValueAsync(response);
        Assert.Equal(team.Id.ToString(), value?["id"]?.GetValue<string>());
        Assert.Equal("Team Alpha", value?["name"]?.GetValue<string>());
    }

    [Fact]
    public async Task GetTeam_ReturnsNotFound_WhenTeamDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/teams/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // POST /api/teams

    [Fact]
    public async Task CreateTeam_ReturnsCreatedTeam_WhenRequestIsValid()
    {
        var tournament = await SeedTournamentAsync();

        var response = await _client.PostAsJsonAsync("/api/teams", new
        {
            TournamentId = tournament.Id,
            Name = "Team Alpha"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var value = await GetResponseValueAsync(response);
        Assert.Equal("Team Alpha", value?["name"]?.GetValue<string>());
        Assert.Equal(tournament.Id.ToString(), value?["tournamentId"]?.GetValue<string>());
    }

    [Fact]
    public async Task CreateTeam_ReturnsNotFound_WhenTournamentDoesNotExist()
    {
        var response = await _client.PostAsJsonAsync("/api/teams", new
        {
            TournamentId = Guid.NewGuid(),
            Name = "Team Alpha"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTeam_ReturnsBadRequest_WhenTournamentIsNotInPreparation()
    {
        var tournament = await SeedTournamentAsync();
        await SetTournamentStatusAsync(tournament.Id, TournamentStatus.GroupStage);

        var response = await _client.PostAsJsonAsync("/api/teams", new
        {
            TournamentId = tournament.Id,
            Name = "Team Alpha"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTeam_ReturnsBadRequest_WhenTeamNameIsDuplicate()
    {
        var tournament = await SeedTournamentAsync();
        await SeedTeamAsync(tournament.Id, "Team Alpha");

        var response = await _client.PostAsJsonAsync("/api/teams", new
        {
            TournamentId = tournament.Id,
            Name = "Team Alpha"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTeam_ReturnsBadRequest_WhenTeamNameIsEmpty()
    {
        var tournament = await SeedTournamentAsync();

        var response = await _client.PostAsJsonAsync("/api/teams", new
        {
            TournamentId = tournament.Id,
            Name = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTeam_ReturnsBadRequest_WhenMaxTeamsReached()
    {
        var tournament = await SeedTournamentAsync();

        await _factory.ExecuteDbContextAsync(async db =>
        {
            for (var i = 1; i <= TournamentConstraints.TeamsRequiredCount; i++)
            {
                var team = Team.Create($"Team {i}", tournament.Id).Value;
                db.Teams.Add(team);
            }
            await db.SaveChangesAsync();
        });

        var response = await _client.PostAsJsonAsync("/api/teams", new
        {
            TournamentId = tournament.Id,
            Name = "Team Extra"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // PUT /api/teams/{id}

    [Fact]
    public async Task UpdateTeamName_ReturnsUpdatedTeam_WhenRequestIsValid()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id, "Team Alpha");

        var response = await _client.PutAsJsonAsync($"/api/teams/{team.Id}", new
        {
            Id = team.Id,
            Name = "Team Updated"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await GetResponseValueAsync(response);
        Assert.Equal("Team Updated", value?["name"]?.GetValue<string>());
    }

    [Fact]
    public async Task UpdateTeamName_ReturnsNotFound_WhenTeamDoesNotExist()
    {
        var response = await _client.PutAsJsonAsync($"/api/teams/{Guid.NewGuid()}", new
        {
            Id = Guid.NewGuid(),
            Name = "Team Updated"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTeamName_ReturnsBadRequest_WhenNameIsDuplicate()
    {
        var tournament = await SeedTournamentAsync();
        await SeedTeamAsync(tournament.Id, "Team Alpha");
        var team2 = await SeedTeamAsync(tournament.Id, "Team Bravo");

        var response = await _client.PutAsJsonAsync($"/api/teams/{team2.Id}", new
        {
            Id = team2.Id,
            Name = "Team Alpha"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // POST /api/teams/{teamId}/players

    [Fact]
    public async Task AddPlayer_ReturnsCreatedPlayer_WhenRequestIsValid()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);

        var response = await _client.PostAsJsonAsync($"/api/teams/{team.Id}/players", new
        {
            TeamId = team.Id,
            Name = "Player One"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var value = await GetResponseValueAsync(response);
        Assert.Equal("Player One", value?["name"]?.GetValue<string>());
    }

    [Fact]
    public async Task AddPlayer_ReturnsNotFound_WhenTeamDoesNotExist()
    {
        var response = await _client.PostAsJsonAsync($"/api/teams/{Guid.NewGuid()}/players", new
        {
            TeamId = Guid.NewGuid(),
            Name = "Player One"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddPlayer_ReturnsBadRequest_WhenTournamentIsNotInPreparation()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        await SetTournamentStatusAsync(tournament.Id, TournamentStatus.GroupStage);

        var response = await _client.PostAsJsonAsync($"/api/teams/{team.Id}/players", new
        {
            TeamId = team.Id,
            Name = "Player One"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddPlayer_ReturnsBadRequest_WhenMaxPlayersReached()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);

        await _factory.ExecuteDbContextAsync(async db =>
        {
            var trackedTeam = await db.Teams.Include(t => t.Players).SingleAsync(t => t.Id == team.Id);
            for (var i = 1; i <= TeamConstraints.PlayersMaxCount; i++)
                trackedTeam.AddPlayer($"Player {i}");
            await db.SaveChangesAsync();
        });

        var response = await _client.PostAsJsonAsync($"/api/teams/{team.Id}/players", new
        {
            TeamId = team.Id,
            Name = "Extra Player"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // PUT /api/teams/{teamId}/players/{playerId}

    [Fact]
    public async Task UpdatePlayerName_ReturnsUpdatedPlayer_WhenRequestIsValid()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        var addResponse = await _client.PostAsJsonAsync($"/api/teams/{team.Id}/players", new
        {
            TeamId = team.Id,
            Name = "Player One"
        });
        var playerId = Guid.Parse(
            (await GetResponseValueAsync(addResponse))?["id"]?.GetValue<string>()
            ?? throw new InvalidOperationException("Player not created"));

        var response = await _client.PutAsJsonAsync($"/api/teams/{team.Id}/players/{playerId}", new
        {
            TeamId = team.Id,
            PlayerId = playerId,
            Name = "Player Updated"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await GetResponseValueAsync(response);
        Assert.Equal("Player Updated", value?["name"]?.GetValue<string>());
    }

    [Fact]
    public async Task UpdatePlayerName_ReturnsNotFound_WhenTeamDoesNotExist()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/teams/{Guid.NewGuid()}/players/{Guid.NewGuid()}", new
            {
                TeamId = Guid.NewGuid(),
                PlayerId = Guid.NewGuid(),
                Name = "Player Updated"
            });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // DELETE /api/teams/{teamId}/players/{playerId}

    [Fact]
    public async Task RemovePlayer_ReturnsOk_WhenPlayerExists()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);
        var addResponse = await _client.PostAsJsonAsync($"/api/teams/{team.Id}/players", new
        {
            TeamId = team.Id,
            Name = "Player One"
        });
        var playerId = Guid.Parse(
            (await GetResponseValueAsync(addResponse))?["id"]?.GetValue<string>()
            ?? throw new InvalidOperationException("Player not created"));

        var response = await _client.DeleteAsync($"/api/teams/{team.Id}/players/{playerId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RemovePlayer_ReturnsNotFound_WhenTeamDoesNotExist()
    {
        var response = await _client.DeleteAsync($"/api/teams/{Guid.NewGuid()}/players/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemovePlayer_ReturnsNotFound_WhenPlayerDoesNotExist()
    {
        var tournament = await SeedTournamentAsync();
        var team = await SeedTeamAsync(tournament.Id);

        var response = await _client.DeleteAsync($"/api/teams/{team.Id}/players/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
