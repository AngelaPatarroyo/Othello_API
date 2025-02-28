using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Othello_API.Migrations
{
    /// <inheritdoc />
    public partial class FixLeaderBoardSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaderBoard_AspNetUsers_UserId",
                table: "LeaderBoard");

            migrationBuilder.DropColumn(
                name: "Ranking",
                table: "LeaderBoard");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "LeaderBoard",
                newName: "PlayerId");

            migrationBuilder.RenameColumn(
                name: "LeaderBoardId",
                table: "LeaderBoard",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_LeaderBoard_UserId",
                table: "LeaderBoard",
                newName: "IX_LeaderBoard_PlayerId");

            migrationBuilder.AddColumn<int>(
                name: "Wins",
                table: "LeaderBoard",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaderBoard_AspNetUsers_PlayerId",
                table: "LeaderBoard",
                column: "PlayerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaderBoard_AspNetUsers_PlayerId",
                table: "LeaderBoard");

            migrationBuilder.DropColumn(
                name: "Wins",
                table: "LeaderBoard");

            migrationBuilder.RenameColumn(
                name: "PlayerId",
                table: "LeaderBoard",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "LeaderBoard",
                newName: "LeaderBoardId");

            migrationBuilder.RenameIndex(
                name: "IX_LeaderBoard_PlayerId",
                table: "LeaderBoard",
                newName: "IX_LeaderBoard_UserId");

            migrationBuilder.AddColumn<int>(
                name: "Ranking",
                table: "LeaderBoard",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaderBoard_AspNetUsers_UserId",
                table: "LeaderBoard",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
