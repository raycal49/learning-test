using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IncidentMonitor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SnakeCaseColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_logs_incidents_IncidentId",
                table: "logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_logs",
                table: "logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_incidents",
                table: "incidents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_alerts",
                table: "alerts");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "logs",
                newName: "timestamp");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "logs",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "logs",
                newName: "level");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "logs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ServiceName",
                table: "logs",
                newName: "service_name");

            migrationBuilder.RenameColumn(
                name: "IncidentId",
                table: "logs",
                newName: "incident_id");

            migrationBuilder.RenameIndex(
                name: "IX_logs_Timestamp",
                table: "logs",
                newName: "IX_logs_timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_logs_ServiceName_Level_Timestamp",
                table: "logs",
                newName: "IX_logs_service_name_level_timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_logs_ServiceName",
                table: "logs",
                newName: "IX_logs_service_name");

            migrationBuilder.RenameIndex(
                name: "IX_logs_IncidentId",
                table: "logs",
                newName: "i_x_logs_incident_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "incidents",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ServiceName",
                table: "incidents",
                newName: "service_name");

            migrationBuilder.RenameColumn(
                name: "OccurrenceCount",
                table: "incidents",
                newName: "occurrence_count");

            migrationBuilder.RenameColumn(
                name: "MessagePattern",
                table: "incidents",
                newName: "message_pattern");

            migrationBuilder.RenameColumn(
                name: "LastSeen",
                table: "incidents",
                newName: "last_seen");

            migrationBuilder.RenameColumn(
                name: "IsResolved",
                table: "incidents",
                newName: "is_resolved");

            migrationBuilder.RenameColumn(
                name: "FirstSeen",
                table: "incidents",
                newName: "first_seen");

            migrationBuilder.RenameIndex(
                name: "IX_incidents_ServiceName_IsResolved",
                table: "incidents",
                newName: "IX_incidents_service_name_is_resolved");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "alerts",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "alerts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TriggeredAt",
                table: "alerts",
                newName: "triggered_at");

            migrationBuilder.RenameColumn(
                name: "ServiceName",
                table: "alerts",
                newName: "service_name");

            migrationBuilder.RenameColumn(
                name: "IsAcknowledged",
                table: "alerts",
                newName: "is_acknowledged");

            migrationBuilder.RenameIndex(
                name: "IX_alerts_TriggeredAt",
                table: "alerts",
                newName: "IX_alerts_triggered_at");

            migrationBuilder.RenameIndex(
                name: "IX_alerts_IsAcknowledged",
                table: "alerts",
                newName: "IX_alerts_is_acknowledged");

            migrationBuilder.AddPrimaryKey(
                name: "p_k_logs",
                table: "logs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "p_k_incidents",
                table: "incidents",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "p_k_alerts",
                table: "alerts",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_logs_incidents_incident_id",
                table: "logs",
                column: "incident_id",
                principalTable: "incidents",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_logs_incidents_incident_id",
                table: "logs");

            migrationBuilder.DropPrimaryKey(
                name: "p_k_logs",
                table: "logs");

            migrationBuilder.DropPrimaryKey(
                name: "p_k_incidents",
                table: "incidents");

            migrationBuilder.DropPrimaryKey(
                name: "p_k_alerts",
                table: "alerts");

            migrationBuilder.RenameColumn(
                name: "timestamp",
                table: "logs",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "logs",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "level",
                table: "logs",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "logs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "service_name",
                table: "logs",
                newName: "ServiceName");

            migrationBuilder.RenameColumn(
                name: "incident_id",
                table: "logs",
                newName: "IncidentId");

            migrationBuilder.RenameIndex(
                name: "IX_logs_timestamp",
                table: "logs",
                newName: "IX_logs_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_logs_service_name_level_timestamp",
                table: "logs",
                newName: "IX_logs_ServiceName_Level_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_logs_service_name",
                table: "logs",
                newName: "IX_logs_ServiceName");

            migrationBuilder.RenameIndex(
                name: "i_x_logs_incident_id",
                table: "logs",
                newName: "IX_logs_IncidentId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "incidents",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "service_name",
                table: "incidents",
                newName: "ServiceName");

            migrationBuilder.RenameColumn(
                name: "occurrence_count",
                table: "incidents",
                newName: "OccurrenceCount");

            migrationBuilder.RenameColumn(
                name: "message_pattern",
                table: "incidents",
                newName: "MessagePattern");

            migrationBuilder.RenameColumn(
                name: "last_seen",
                table: "incidents",
                newName: "LastSeen");

            migrationBuilder.RenameColumn(
                name: "is_resolved",
                table: "incidents",
                newName: "IsResolved");

            migrationBuilder.RenameColumn(
                name: "first_seen",
                table: "incidents",
                newName: "FirstSeen");

            migrationBuilder.RenameIndex(
                name: "IX_incidents_service_name_is_resolved",
                table: "incidents",
                newName: "IX_incidents_ServiceName_IsResolved");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "alerts",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "alerts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "triggered_at",
                table: "alerts",
                newName: "TriggeredAt");

            migrationBuilder.RenameColumn(
                name: "service_name",
                table: "alerts",
                newName: "ServiceName");

            migrationBuilder.RenameColumn(
                name: "is_acknowledged",
                table: "alerts",
                newName: "IsAcknowledged");

            migrationBuilder.RenameIndex(
                name: "IX_alerts_triggered_at",
                table: "alerts",
                newName: "IX_alerts_TriggeredAt");

            migrationBuilder.RenameIndex(
                name: "IX_alerts_is_acknowledged",
                table: "alerts",
                newName: "IX_alerts_IsAcknowledged");

            migrationBuilder.AddPrimaryKey(
                name: "PK_logs",
                table: "logs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_incidents",
                table: "incidents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_alerts",
                table: "alerts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_logs_incidents_IncidentId",
                table: "logs",
                column: "IncidentId",
                principalTable: "incidents",
                principalColumn: "Id");
        }
    }
}
