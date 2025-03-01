using Microsoft.EntityFrameworkCore.Migrations;

namespace Othello_API.Migrations
{
    public partial class FixGameStatusInCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create a new column for Ranking with nullable support
            migrationBuilder.AddColumn<int>(
                name: "Ranking_New",
                table: "LeaderBoard",
                type: "INTEGER",
                nullable: true);

            // Step 2: Copy existing data to the new column
            migrationBuilder.Sql("UPDATE LeaderBoard SET Ranking_New = Ranking");

            // Step 3: Drop the old column
            migrationBuilder.DropColumn(
                name: "Ranking",
                table: "LeaderBoard");

            // Step 4: Rename the new column to match the old one
            migrationBuilder.RenameColumn(
                name: "Ranking_New",
                table: "LeaderBoard",
                newName: "Ranking");

            // Adding WinnerId column for Games
            migrationBuilder.AddColumn<string>(
                name: "WinnerId",
                table: "Games",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_WinnerId",
                table: "Games",
                column: "WinnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_AspNetUsers_WinnerId",
                table: "Games",
                column: "WinnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create a new column to revert changes
            migrationBuilder.AddColumn<int>(
                name: "Ranking_Old",
                table: "LeaderBoard",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // Step 2: Copy data back to the old column
            migrationBuilder.Sql("UPDATE LeaderBoard SET Ranking_Old = Ranking");

            // Step 3: Drop the modified column
            migrationBuilder.DropColumn(name: "Ranking", table: "LeaderBoard");

            // Step 4: Rename the old column back to its original name
            migrationBuilder.RenameColumn(name: "Ranking_Old", table: "LeaderBoard", newName: "Ranking");

            // Remove WinnerId from Games
            migrationBuilder.DropForeignKey(
                name: "FK_Games_AspNetUsers_WinnerId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_WinnerId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "WinnerId",
                table: "Games");
        }
    }
}
