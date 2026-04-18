using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineTicketing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PreDeployRiskFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Flights_FlightNumber",
                table: "Flights");

            migrationBuilder.AddColumn<int>(
                name: "FlightId",
                table: "CheckIns",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "CheckIns" AS c
                SET "FlightId" = t."FlightId"
                FROM "Tickets" AS t
                WHERE c."TicketId" = t."Id";
                """);

            migrationBuilder.AlterColumn<int>(
                name: "FlightId",
                table: "CheckIns",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flights_FlightNumber_DepartureTime",
                table: "Flights",
                columns: new[] { "FlightNumber", "DepartureTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_FlightId_SeatNumber",
                table: "CheckIns",
                columns: new[] { "FlightId", "SeatNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CheckIns_Flights_FlightId",
                table: "CheckIns",
                column: "FlightId",
                principalTable: "Flights",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckIns_Flights_FlightId",
                table: "CheckIns");

            migrationBuilder.DropIndex(
                name: "IX_Flights_FlightNumber_DepartureTime",
                table: "Flights");

            migrationBuilder.DropIndex(
                name: "IX_CheckIns_FlightId_SeatNumber",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "FlightId",
                table: "CheckIns");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_FlightNumber",
                table: "Flights",
                column: "FlightNumber",
                unique: true);
        }
    }
}
