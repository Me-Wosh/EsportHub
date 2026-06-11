# Team

## Description

A Team represents a group of players competing in the tournament. Teams and their rosters are managed before the tournament officially starts.

## Constraints

- **Player Limits**: A team must have a minimum of 5 players to participate in a tournament. A team can have a maximum of 10 players.

- **Limit Validation**: When a user tries to add a player exceeding the 10-player limit, the system gracefully rejects the action with an appropriate informational message, rather than breaking tournament participation logic.

- **Modification Window**: Creating, updating, and deleting a team, as well as managing team players (create, update, delete), are strictly restricted to the period *before* the tournament starts. Once the tournament begins, rosters are locked.

## Entity

|Column|Data type|Notes|
|---|---|---|
|`Id`|GUID|Derived from BaseEntity|
|`CreatedAt`|DateTime|Derived from BaseEntity|
|`UpdatedAt`|DateTime|Derived from BaseEntity|
|`Name`|String|Required, Max Length: 100|
|`TournamentId`|GUID|Foreign key to Tournament entity|
|`Players`|Collection|Collection of Player entities|

### Player (Sub-Entity)

|Column|Data type|Notes|
|---|---|---|
|`Id`|GUID|Derived from BaseEntity|
|`CreatedAt`|DateTime|Derived from BaseEntity|
|`UpdatedAt`|DateTime|Derived from BaseEntity|
|`Name`|String|Required, Max Length: 100|
|`TeamId`|GUID|Foreign key to Team entity|
