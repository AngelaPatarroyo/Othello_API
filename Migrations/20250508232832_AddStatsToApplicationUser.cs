using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Othello_API.Migrations
{
    /// <inheritdoc />
    public partial class AddStatsToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Draws",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Losses",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalGames",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "WinRate",
                table: "AspNetUsers",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Wins",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Draws",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Losses",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TotalGames",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WinRate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Wins",
                table: "AspNetUsers");
        }
    }
}
