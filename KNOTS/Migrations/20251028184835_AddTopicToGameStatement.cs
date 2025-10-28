using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KNOTS.Migrations
{
    /// <inheritdoc />
    public partial class AddTopicToGameStatement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoomCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    PlayedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    TotalPlayers = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerUsernames = table.Column<string>(type: "TEXT", nullable: false),
                    BestMatchPlayer = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BestMatchPercentage = table.Column<double>(type: "REAL", nullable: false),
                    ResultsJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSwipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoomCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    PlayerUsername = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StatementId = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    StatementText = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    AgreeWithStatement = table.Column<bool>(type: "INTEGER", nullable: false),
                    SwipedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSwipes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Statements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Topic = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    TotalGamesPlayed = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    AverageCompatibilityScore = table.Column<double>(type: "REAL", nullable: false, defaultValue: 0.0),
                    BestMatchesCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Username);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSwipes_RoomCode_PlayerUsername_StatementId",
                table: "PlayerSwipes",
                columns: new[] { "RoomCode", "PlayerUsername", "StatementId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameHistory");

            migrationBuilder.DropTable(
                name: "PlayerSwipes");

            migrationBuilder.DropTable(
                name: "Statements");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
