# Group Stage

## Description

The first competitive phase of the tournament. The 16 teams are divided into 4 groups of 4 teams each. Teams play against every other team in their group once.

## Constraints

- **Game Distribution**: Each group plays a round-robin format, resulting in 6 games per group (24 games total for the Group Stage). A team cannot play against itself.

- **Planning**: When the group stage starts (automatically triggered by the tournament start), all games are immediately planned and saved in the database with `null` scores.

- **Tie-breakers & Standings**: The top 2 teams from each group advance to the Knockout Stage. Standings are calculated in the following order:

  1. Win-Loss record.

  2. Point differential (scored points minus lost points - it's better to score 6 and lose 3 than score 10 and lose 14).
  
  3. Head-to-Head winner (if 2 teams are tied).
  
  4. If 3 teams are tied: Standings are recalculated using only the matching games played between these 3 teams (applying steps 1-3).
  
  5. If the winner still cannot be uniquely decided, the winner is picked randomly.

- **Closing**: Closing the group stage is a manual action, which can only be executed after all games in the group stage have been successfully played and resolved. Closing automatically initiates the Knockout Stage.

## Entity

|Column|Data type|Notes|
|---|---|---|
|`Id`|GUID|Derived from BaseEntity|
|`CreatedAt`|DateTime|Derived from BaseEntity|
|`UpdatedAt`|DateTime|Derived from BaseEntity|
|`TournamentId`|GUID||
|`IsClosed`|Boolean|Default: false|
|`Groups`|Collection|Collection of 4 groups, e.g., Group A, B, C, D|

### Group (Sub-Entity)

|Column|Data type|Notes|
|---|---|---|
|`Id`|GUID|Derived from BaseEntity|
|`CreatedAt`|DateTime|Derived from BaseEntity|
|`UpdatedAt`|DateTime|Derived from BaseEntity|
|`Name`|String|Required, Max Length: 20 (e.g., A, B, C, D)|
|`TeamIds`|Collection|Collection of 4 Team IDs participating in this group|
|`Games`|Collection|Collection of planned Game IDs for this group (6 games)|
