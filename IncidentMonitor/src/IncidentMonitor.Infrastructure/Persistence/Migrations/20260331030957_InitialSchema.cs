using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IncidentMonitor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TriggeredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MessagePattern = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OccurrenceCount = table.Column<int>(type: "integer", nullable: false),
                    FirstSeen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incidents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_logs_incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "incidents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_alerts_IsAcknowledged",
                table: "alerts",
                column: "IsAcknowledged");

            migrationBuilder.CreateIndex(
                name: "IX_alerts_TriggeredAt",
                table: "alerts",
                column: "TriggeredAt");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_ServiceName_IsResolved",
                table: "incidents",
                columns: new[] { "ServiceName", "IsResolved" });

            migrationBuilder.CreateIndex(
                name: "IX_logs_IncidentId",
                table: "logs",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_logs_ServiceName",
                table: "logs",
                column: "ServiceName");

            migrationBuilder.CreateIndex(
                name: "IX_logs_ServiceName_Level_Timestamp",
                table: "logs",
                columns: new[] { "ServiceName", "Level", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_logs_Timestamp",
                table: "logs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alerts");

            migrationBuilder.DropTable(
                name: "logs");

            migrationBuilder.DropTable(
                name: "incidents");
        }
    }
}
