using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsportHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueIndexOnGroupTeamStandingPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GroupTeamStandings_GroupId_Position",
                table: "GroupTeamStandings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_GroupTeamStandings_GroupId_Position",
                table: "GroupTeamStandings",
                columns: new[] { "GroupId", "Position" },
                unique: true);
        }
    }
}
