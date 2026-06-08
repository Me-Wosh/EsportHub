# Game

## Description

A single match played betwenn two team, either during the Group Stage or the Knockout Stage.
Table-Per-Hierarchy (TPH) approach: a single `Game` table stores both Group Stage and Knockout Stage games with `StageType` field being the discriminator.

## Constraints

- **Initialization**: Games are created automatically during the planning phases of their respective stages (group or knockout stage) with `null` scores.

- **Scoring**: A game result is represented by two integers (one for each team). The team with the higher score is the winner.

- **No Draws Allowed**: A game must always have a distinct winner and loser. Tied scores are forbidden.

- **Completion**: Finishing a game is a manual action executed by a user, during which the final scores are submitted and saved, immediately updating the game's state.

- **Editing Window**: After the scores are submitted, any changes to the game are prohibited.

- **Strict Editing**: There is no other way of editing this entity than setting the scores. All other properties are set automatically by the system and are not manually modified.

- **Score Consistency**: `Score1` and `Score2` must be either both `null` (initial/unresolved state) or both non-`null` (resolved state). Partial score states (one null, one set) are invalid.

## Entity (TPH)

|Column|Data type|Notes|
|---|---|---|
|`Id`|GUID|Derived from BaseEntity|
|`CreatedAt`|DateTime|Derived from BaseEntity|
|`UpdatedAt`|DateTime|Derived from BaseEntity|
|`StageType`|Enum|Discriminator - Values: Group, Knockout|
|`Team1Id`|GUID||
|`Team2Id`|GUID||
|`Score1`|Integer|Nullable, Minimum: 0 - represents Team 1's score|
|`Score2`|Integer|Nullable, Minimum: 0 - represents Team 2's score|

### GroupStageGame (Subtype)

|Column|Data type|Notes|
|---|---|---|
|`GroupId`|GUID|References the Group this game belongs to|

### KnockoutStageGame (Subtype)

|Column|Data type|Notes|
|---|---|---|
|`KnockoutStageId`|GUID|References the Knockout Stage bracket/instance this game belongs to|
|`Side`|Enum|Values: Left, Right - indicates bracket side for progression logic|
