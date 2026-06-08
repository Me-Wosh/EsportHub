# Knockout Stage

## Description

The final phase of the tournament using a single-elimination bracket. The top 8 teams from the Group Stage compete in Quarter Finals, Semi Finals, and the Final.

## Constraints

- **Quarter Finals Initialization**: At the start of the knockout stage (triggered by closing the Group Stage), Quarter Final matches are randomly generated between the advancing teams. The games are saved in the database instantly with `null` scores.

- **Bracket Progression**:

  - When two connected Quarter Final games on the same side of the bracket are resolved via manual action, the corresponding Semi Final game is automatically planned/created in the database with `null` scores.
  
  - Resolving a Semi Final game naturally plans the Final game using the identical automated mechanism.

- **Tournament Completion**: Resolving the Final game automatically marks the entire Tournament as closed.

## Entity

|Column|Data type|Notes|
|---|---|---|
|`Id`|GUID|Derived from BaseEntity|
|`CreatedAt`|DateTime|Derived from BaseEntity|
|`UpdatedAt`|DateTime|Derived from BaseEntity|
|`TournamentId`|GUID||
|`IsClosed`|Boolean|Default: false|
