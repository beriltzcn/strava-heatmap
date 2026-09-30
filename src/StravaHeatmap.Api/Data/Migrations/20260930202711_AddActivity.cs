using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StravaHeatmap.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StravaActivityId = table.Column<long>(type: "INTEGER", nullable: false),
                    StravaConnectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    SportType = table.Column<string>(type: "TEXT", nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DistanceMeters = table.Column<double>(type: "REAL", nullable: false),
                    MovingTimeSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    ElapsedTimeSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalElevationGain = table.Column<double>(type: "REAL", nullable: false),
                    AverageSpeed = table.Column<double>(type: "REAL", nullable: false),
                    SummaryPolyline = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activities_StravaConnections_StravaConnectionId",
                        column: x => x.StravaConnectionId,
                        principalTable: "StravaConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_StravaActivityId",
                table: "Activities",
                column: "StravaActivityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_StravaConnectionId",
                table: "Activities",
                column: "StravaConnectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities");
        }
    }
}
