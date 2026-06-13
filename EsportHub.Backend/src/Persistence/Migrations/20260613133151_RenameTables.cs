using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsportHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Group_GroupStage_GroupStageId",
                table: "Group");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupStage_Tournament_TournamentId",
                table: "GroupStage");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupTeamStanding_Group_GroupId",
                table: "GroupTeamStanding");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupTeamStanding_Team_TeamId",
                table: "GroupTeamStanding");

            migrationBuilder.DropForeignKey(
                name: "FK_KnockoutStage_Tournament_TournamentId",
                table: "KnockoutStage");

            migrationBuilder.DropForeignKey(
                name: "FK_Match_Group_GroupId",
                table: "Match");

            migrationBuilder.DropForeignKey(
                name: "FK_Match_KnockoutStage_KnockoutStageId",
                table: "Match");

            migrationBuilder.DropForeignKey(
                name: "FK_Match_Team_Team1Id",
                table: "Match");

            migrationBuilder.DropForeignKey(
                name: "FK_Match_Team_Team2Id",
                table: "Match");

            migrationBuilder.DropForeignKey(
                name: "FK_Player_Team_TeamId",
                table: "Player");

            migrationBuilder.DropForeignKey(
                name: "FK_Team_Group_GroupId",
                table: "Team");

            migrationBuilder.DropForeignKey(
                name: "FK_Team_Tournament_TournamentId",
                table: "Team");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournament",
                table: "Tournament");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Team",
                table: "Team");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Player",
                table: "Player");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Match",
                table: "Match");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KnockoutStage",
                table: "KnockoutStage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupTeamStanding",
                table: "GroupTeamStanding");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupStage",
                table: "GroupStage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Group",
                table: "Group");

            migrationBuilder.RenameTable(
                name: "Tournament",
                newName: "Tournaments");

            migrationBuilder.RenameTable(
                name: "Team",
                newName: "Teams");

            migrationBuilder.RenameTable(
                name: "Player",
                newName: "Players");

            migrationBuilder.RenameTable(
                name: "Match",
                newName: "Matches");

            migrationBuilder.RenameTable(
                name: "KnockoutStage",
                newName: "KnockoutStages");

            migrationBuilder.RenameTable(
                name: "GroupTeamStanding",
                newName: "GroupTeamStandings");

            migrationBuilder.RenameTable(
                name: "GroupStage",
                newName: "GroupStages");

            migrationBuilder.RenameTable(
                name: "Group",
                newName: "Groups");

            migrationBuilder.RenameIndex(
                name: "IX_Team_TournamentId",
                table: "Teams",
                newName: "IX_Teams_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_Team_GroupId",
                table: "Teams",
                newName: "IX_Teams_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Player_TeamId",
                table: "Players",
                newName: "IX_Players_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_Match_Team2Id",
                table: "Matches",
                newName: "IX_Matches_Team2Id");

            migrationBuilder.RenameIndex(
                name: "IX_Match_Team1Id",
                table: "Matches",
                newName: "IX_Matches_Team1Id");

            migrationBuilder.RenameIndex(
                name: "IX_Match_KnockoutStageId",
                table: "Matches",
                newName: "IX_Matches_KnockoutStageId");

            migrationBuilder.RenameIndex(
                name: "IX_Match_GroupId",
                table: "Matches",
                newName: "IX_Matches_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_KnockoutStage_TournamentId",
                table: "KnockoutStages",
                newName: "IX_KnockoutStages_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupTeamStanding_TeamId",
                table: "GroupTeamStandings",
                newName: "IX_GroupTeamStandings_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupTeamStanding_GroupId_TeamId",
                table: "GroupTeamStandings",
                newName: "IX_GroupTeamStandings_GroupId_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupTeamStanding_GroupId_Position",
                table: "GroupTeamStandings",
                newName: "IX_GroupTeamStandings_GroupId_Position");

            migrationBuilder.RenameIndex(
                name: "IX_GroupStage_TournamentId",
                table: "GroupStages",
                newName: "IX_GroupStages_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_Group_GroupStageId",
                table: "Groups",
                newName: "IX_Groups_GroupStageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Teams",
                table: "Teams",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Players",
                table: "Players",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Matches",
                table: "Matches",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KnockoutStages",
                table: "KnockoutStages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupTeamStandings",
                table: "GroupTeamStandings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupStages",
                table: "GroupStages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Groups",
                table: "Groups",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_GroupStages_GroupStageId",
                table: "Groups",
                column: "GroupStageId",
                principalTable: "GroupStages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupStages_Tournaments_TournamentId",
                table: "GroupStages",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTeamStandings_Groups_GroupId",
                table: "GroupTeamStandings",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTeamStandings_Teams_TeamId",
                table: "GroupTeamStandings",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KnockoutStages_Tournaments_TournamentId",
                table: "KnockoutStages",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Groups_GroupId",
                table: "Matches",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_KnockoutStages_KnockoutStageId",
                table: "Matches",
                column: "KnockoutStageId",
                principalTable: "KnockoutStages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Teams_Team1Id",
                table: "Matches",
                column: "Team1Id",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Teams_Team2Id",
                table: "Matches",
                column: "Team2Id",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Teams_TeamId",
                table: "Players",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Groups_GroupId",
                table: "Teams",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Tournaments_TournamentId",
                table: "Teams",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_GroupStages_GroupStageId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupStages_Tournaments_TournamentId",
                table: "GroupStages");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupTeamStandings_Groups_GroupId",
                table: "GroupTeamStandings");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupTeamStandings_Teams_TeamId",
                table: "GroupTeamStandings");

            migrationBuilder.DropForeignKey(
                name: "FK_KnockoutStages_Tournaments_TournamentId",
                table: "KnockoutStages");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Groups_GroupId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_KnockoutStages_KnockoutStageId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Teams_Team1Id",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Teams_Team2Id",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_Teams_TeamId",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Groups_GroupId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Tournaments_TournamentId",
                table: "Teams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Teams",
                table: "Teams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Players",
                table: "Players");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Matches",
                table: "Matches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KnockoutStages",
                table: "KnockoutStages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupTeamStandings",
                table: "GroupTeamStandings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupStages",
                table: "GroupStages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Groups",
                table: "Groups");

            migrationBuilder.RenameTable(
                name: "Tournaments",
                newName: "Tournament");

            migrationBuilder.RenameTable(
                name: "Teams",
                newName: "Team");

            migrationBuilder.RenameTable(
                name: "Players",
                newName: "Player");

            migrationBuilder.RenameTable(
                name: "Matches",
                newName: "Match");

            migrationBuilder.RenameTable(
                name: "KnockoutStages",
                newName: "KnockoutStage");

            migrationBuilder.RenameTable(
                name: "GroupTeamStandings",
                newName: "GroupTeamStanding");

            migrationBuilder.RenameTable(
                name: "GroupStages",
                newName: "GroupStage");

            migrationBuilder.RenameTable(
                name: "Groups",
                newName: "Group");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_TournamentId",
                table: "Team",
                newName: "IX_Team_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_GroupId",
                table: "Team",
                newName: "IX_Team_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Players_TeamId",
                table: "Player",
                newName: "IX_Player_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_Matches_Team2Id",
                table: "Match",
                newName: "IX_Match_Team2Id");

            migrationBuilder.RenameIndex(
                name: "IX_Matches_Team1Id",
                table: "Match",
                newName: "IX_Match_Team1Id");

            migrationBuilder.RenameIndex(
                name: "IX_Matches_KnockoutStageId",
                table: "Match",
                newName: "IX_Match_KnockoutStageId");

            migrationBuilder.RenameIndex(
                name: "IX_Matches_GroupId",
                table: "Match",
                newName: "IX_Match_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_KnockoutStages_TournamentId",
                table: "KnockoutStage",
                newName: "IX_KnockoutStage_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupTeamStandings_TeamId",
                table: "GroupTeamStanding",
                newName: "IX_GroupTeamStanding_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupTeamStandings_GroupId_TeamId",
                table: "GroupTeamStanding",
                newName: "IX_GroupTeamStanding_GroupId_TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupTeamStandings_GroupId_Position",
                table: "GroupTeamStanding",
                newName: "IX_GroupTeamStanding_GroupId_Position");

            migrationBuilder.RenameIndex(
                name: "IX_GroupStages_TournamentId",
                table: "GroupStage",
                newName: "IX_GroupStage_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_GroupStageId",
                table: "Group",
                newName: "IX_Group_GroupStageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournament",
                table: "Tournament",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Team",
                table: "Team",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Player",
                table: "Player",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Match",
                table: "Match",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KnockoutStage",
                table: "KnockoutStage",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupTeamStanding",
                table: "GroupTeamStanding",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupStage",
                table: "GroupStage",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Group",
                table: "Group",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Group_GroupStage_GroupStageId",
                table: "Group",
                column: "GroupStageId",
                principalTable: "GroupStage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupStage_Tournament_TournamentId",
                table: "GroupStage",
                column: "TournamentId",
                principalTable: "Tournament",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTeamStanding_Group_GroupId",
                table: "GroupTeamStanding",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTeamStanding_Team_TeamId",
                table: "GroupTeamStanding",
                column: "TeamId",
                principalTable: "Team",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KnockoutStage_Tournament_TournamentId",
                table: "KnockoutStage",
                column: "TournamentId",
                principalTable: "Tournament",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Match_Group_GroupId",
                table: "Match",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Match_KnockoutStage_KnockoutStageId",
                table: "Match",
                column: "KnockoutStageId",
                principalTable: "KnockoutStage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Match_Team_Team1Id",
                table: "Match",
                column: "Team1Id",
                principalTable: "Team",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Match_Team_Team2Id",
                table: "Match",
                column: "Team2Id",
                principalTable: "Team",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Player_Team_TeamId",
                table: "Player",
                column: "TeamId",
                principalTable: "Team",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Team_Group_GroupId",
                table: "Team",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Team_Tournament_TournamentId",
                table: "Team",
                column: "TournamentId",
                principalTable: "Tournament",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
