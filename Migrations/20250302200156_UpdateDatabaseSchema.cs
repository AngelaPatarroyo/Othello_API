using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Othello_API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaderBoard_AspNetUsers_PlayerId",
                table: "LeaderBoard");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaderBoard_AspNetUsers_PlayerId",
                table: "LeaderBoard",
                column: "PlayerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaderBoard_AspNetUsers_PlayerId",
                table: "LeaderBoard");

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
