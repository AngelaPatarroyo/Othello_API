using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Othello_API.Migrations
{
    /// <inheritdoc />
    public partial class RenameLeaderBoardsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaderBoard_AspNetUsers_PlayerId",
                table: "LeaderBoard");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaderBoard",
                table: "LeaderBoard");

            migrationBuilder.RenameTable(
                name: "LeaderBoard",
                newName: "LeaderBoard");

            migrationBuilder.RenameIndex(
                name: "IX_LeaderBoard_PlayerId",
                table: "LeaderBoard",
                newName: "IX_LeaderBoard_PlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaderBoard",
                table: "LeaderBoard",
                column: "Id");

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

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaderBoard",
                table: "LeaderBoard");

            migrationBuilder.RenameTable(
                name: "LeaderBoard",
                newName: "LeaderBoards");

            migrationBuilder.RenameIndex(
                name: "IX_LeaderBoard_PlayerId",
                table: "LeaderBoard",
                newName: "IX_LeaderBoard_PlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaderBoard",
                table: "LeaderBoard",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaderBoard_AspNetUsers_PlayerId",
                table: "LeaderBoard",
                column: "PlayerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
