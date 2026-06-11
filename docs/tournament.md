# Tournament

## Description

A Tournament is the core entity that orchestrates the entire event. It oversees the progression of teams from the Group Stage through the Knockout Stage to determine a final champion.

## Constraints

- **Team Requirements**: Starting a tournament requires exactly 16 teams.

- **Roster Validation**: Every participating team must have a complete roster (minimum 5 players). Trying to start the tournament without fulfilling these conditions fails with an appropriate message.

- **Manual Start**: Starting the tournament is a manual action triggered by the user.

- **State Mechanics**: 

  - The tournament starts in the `InPreparation` state.

  - Starting the tournament immediately marks the `StartDate` and automatically transitions the state to `GroupStage`.

  - Closing the group stage automatically transitions the state to `KnockoutStage`.

  - The tournament state is automatically changed to `Finished` when the final match in the Knockout stage is resolved.

## Entity

|Column|Data type|Notes|
|---|---|---|
|`Id`|GUID|Derived from BaseEntity|
|`CreatedAt`|DateTime|Derived from BaseEntity|
|`UpdatedAt`|DateTime|Derived from BaseEntity|
|`Name`|String|Required, Max Length: 100|
|`StartDate`|DateTime|Nullable|
|`Status`|Enum|Values: InPreparation, GroupStage, KnockoutStage, Finished. Default: InPreparation|
|`GroupStage`|GroupStage|Nullable, navigation to the GroupStage entity|
|`KnockoutStage`|KnockoutStage|Nullable, navigation to the KnockoutStage entity|
