# Knockout Stage

## Description

The final phase of the tournament using a single-elimination bracket. The top 8 teams from the Group Stage compete in Quarter Finals, Semi Finals, and the Final.

## Constraints

- **Quarter Finals Initialization**: At the start of the knockout stage (triggered by closing the Group Stage), Quarter Final matches are generated between the advancing teams. Teams from the same group are guaranteed not to face each other in the Quarter Finals. Pairings are otherwise random. The matches are saved in the database instantly with `null` scores. Each match is assigned a bracket side (Left or Right).

- **Bracket Progression**:

  - When all four Quarter Final matches are resolved, both Semi Final matches are automatically created: one for the Left bracket winners and one for the Right bracket winners.

  - When both Semi Final matches are resolved, the Final match is automatically created.

- **Tournament Completion**: Resolving the Final match automatically marks the entire Tournament as closed.

## Entity

|Column|Data type|Notes|
|---|---|---|
|`Id`|GUID|Derived from BaseEntity|
|`CreatedAt`|DateTime|Derived from BaseEntity|
|`UpdatedAt`|DateTime|Derived from BaseEntity|
|`TournamentId`|GUID|Foreign key to Tournament entity|
|`IsClosed`|Boolean|Default: false|
|`Matches`|Collection|Navigation to all KnockoutStageMatch entities (Quarter Finals, Semi Finals, Final)|
