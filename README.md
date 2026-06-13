# EsportHub

EsportHub is a backend REST API for managing esports tournaments. It handles the full lifecycle of a competition - from registering teams and players, through the group stage, to the knockout stage and final - with live stream integration via Twitch.

Live: [https://esporthub-lpx3.onrender.com](https://esporthub-lpx3.onrender.com)

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| Web framework | ASP.NET Minimal API |
| ORM | Entity Framework 10 |
| Database | PostgreSQL 18 |
| Containerization | Docker + Docker Compose |

## Running with Docker

### Prerequisites

- Docker and Docker Compose installed,
- a dev HTTPS certificate at `~/.aspnet/https/esporthub.pfx` (generated with `dotnet dev-certs https`).

### 1. Create the `.env` file

Create a `.env` file in the repository root with the following variables:

```env
POSTGRES_DB="EsportHubDb"
POSTGRES_PASSWORD="your_db_password"

EsportHubDb_DockerConnectionString="Host=db;Port=542;Database=esporthub;Username=postgres;Password=your_db_password;Trust Server Certificate=True;"

HTTPS_CERT_PASSWORD="your_cert_password"

MediatR_LicenseKey="your_mediatR_license_key"

Twitch_ClientId="your_twitch_client_id"
Twitch_ClientSecret="your_twitch_client_secret"
Twitch_BroadcasterId="your_broadcaster_id"
Twitch_RedirectUri="https://your_host:your_port/api/auth/twitch/callback"
```

### 2. Start the application

```zsh
docker compose up --build
```

The API will be available at `https://localhost:443` and `http://localhost:80`. The web container waits for the database health check to pass before starting.

### 3. Database migrations

The database schema is managed via EF migrations. Migrations are automatically applied on application startup.

---

## Architecture

### Architecture diagram

```
/EsportHub.Backend
├── .dockerignore
├── coverlet.runsettings - settings for test coverage collector
├── Dockerfile
├── Dockerfile.dev
├── EsportHub.Backend.slnx
├── /src
│   ├── appsettings.Development.json
│   ├── appsettings.json
│   ├── EsportHub.csproj
│   ├── GlobalUsings.cs
│   ├── Program.cs - app entry point
│   ├── /Configuration - registering services, enabling WebApplicationBuilder features
│   ├── /Domain - collection of aggregates and business rules
│   │   ├── /Matches
│   │   ├── /Teams
│   │   ├── /Tournaments
│   │   └── BaseEntity.cs - base entity, all aggregates derive from it
│   ├── /Endpoints
│   │   └── /Filters
│   ├── /Features - handlers for app features
│   │   ├── /LiveStreams
│   │   ├── /Teams
│   │   └── /Tournaments
│   ├── /Infrastructure - external services
│   │   ├── /MediatR
│   │   └── /Twitch
│   ├── /Middleware
│   ├── /Persistence - database layer
│   │   ├── /Configurations - entities configurations
│   │   ├── /Migrations
│   │   └── EsportHubDbContext.cs
│   └── /Properties
│       └── launchSettings.json
└── /tests
    ├── /EsportHub.IntegrationTests
    └── /EsportHub.UnitTests
```

---

### ERD Diagram

![ERD Diagram](./docs/ERD-diagram.png)

---

### Domain-Driven Design and rich domain

The domain layer (`Domain/`) encapsulates all business rules directly inside the aggregate roots and entities. Entities are never anemic data bags - they own their invariants and expose only operations that keep them valid.

Examples of enforced business rules:
- a `Match` cannot end in a draw; scores cannot be changed after the match is resolved,
- a `Team` cannot have more than the allowed maximum number of players, and no two players in the same team can share a name,
- a `Tournament` can only be started with exactly the required number of teams, each of which must meet the minimum roster size,
- a tournament's group stage must be completed before the knockout stage is initialized.

Constructors are private or protected; objects are created through static factory methods that return `Result<T>`, ensuring that an instance can never exist in an invalid state. Properties are set only through methods that validate the change first.

---

### Result Pattern instead of exceptions

The application uses [Ardalis.Result](https://github.com/ardalis/Result) as a first-class return type throughout the entire stack - domain, application, and web layers - instead of throwing exceptions for expected failure cases.

Every operation that can fail returns a typed `Result<T>` or `Result`:

```cs
public Result<Team> UpdateName(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        return Result.Invalid(new ValidationError("Team name cannot be empty."));

    Name = name;
    return this;
}
```

The `ArdalisResultMapper` endpoint filter translates result statuses to HTTP responses automatically, so handlers never call `Results.Ok()` or `Results.BadRequest()` manually.

**Advantages:**
- failures are part of the method signature - callers are forced to handle them,
- no invisible control flow jumps; the call stack is always clean,
- efficiency - exceptions are expensive; returning a failed Result takes less system resources,
- multiple validation errors can be aggregated and returned in a single response,
- chaining via `.Bind()` keeps happy-path code readable without nested `if` blocks,
- cleaner code - failing paths are handled with simple `if (!result.IsSuccess) return result.Map()` call instead of multiple try-catch blocks that introduce unwanted nesting and require handling each exception individually.
- easier to test: assertions on return values instead of catching exceptions.

**Trade-offs:**

- more verbose than `throw` for simple guard clauses,
- requires discipline - mixing exceptions and Results in the same codebase creates inconsistency,
- third-party libraries still throw, so exceptions cannot be eliminated entirely at the infrastructure boundary.

---

### Table Per Hierarchy (TPH) for Match

`Match` is an abstract base class with two concrete subtypes: `GroupStageMatch` and `KnockoutStageMatch`. Both are stored in a single `Matches` table using EF Table Per Hierarchy strategy. The two match types share the majority of their columns, making a single table simpler and more efficient than Table Per Type which would require a JOIN on every read.

A `StageType` discriminator column stores the subtype as a string (`"Group"` or `"Knockout"`), and EF materializes the correct C# type at query time:

```cs
builder.HasDiscriminator(match => match.StageType)
    .HasValue<GroupStageMatch>(MatchStageType.Group)
    .HasValue<KnockoutStageMatch>(MatchStageType.Knockout);
```

Columns specific to one subtype (e.g. `GroupId` for group stage matches, or `Round` and `Side` for knockout matches) are nullable in the shared table. A database-level check constraint ensures score consistency - both scores must be null or both non-null at the same time.

---

### DeleteBehavior.Restrict on relationships

Many table relationships are configured with `DeleteBehavior.Restrict`.

Example:

```cs
builder.HasOne(tournament => tournament.GroupStage)
    .WithOne(groupStage => groupStage.Tournament)
    .OnDelete(DeleteBehavior.Restrict);
```

This means the database will refuse to delete a related entity that is referenced by parent entity. The restriction is enforced at the database level, not only in application code, so it holds regardless of how the data is accessed. For example: a started group stage within a tournament must not be silently removed. This is enforced because in many cases it would be total disaster if child entities got cascade deleted, for example: deleting group stage with groups and group stage matches after tournament started, meaning we completely lost track of tournament history and there can be a tournament with knockout stage but without a group stage. The only exception is deleting all players whenever a team gets deleted because a `Player` entity cannot live on its own in the database.

### Mediator Pattern in endpoints

Endpoints (Minimal API route handlers in `Endpoints/`) are intentionally thin. Each handler receives an `IMediator` dependency and immediately delegates to a command or query object:

```cs
group.MapPost("/", async Task<Result<TeamResult>> (
    [FromBody] CreateTeamCommand command,
    IMediator mediator,
    CancellationToken cancellationToken) =>
{
    return await mediator.Send(command, cancellationToken);
});
```

All business logic lives in the corresponding handler classes under `Features/`. This separation means:
- endpoints have zero business logic and need no unit tests of their own,
- handlers can be tested in isolation without an HTTP stack,
- adding cross-cutting concerns (logging, validation pipelines) is done once in the MediatR pipeline, not in every endpoint.
