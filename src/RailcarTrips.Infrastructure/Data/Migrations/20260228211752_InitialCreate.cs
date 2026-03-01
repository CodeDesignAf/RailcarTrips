using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailcarTrips.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TimeZoneId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipmentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EventUtcTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EventLocalTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NaturalKeyHash = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipmentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OriginCityId = table.Column<int>(type: "INTEGER", nullable: false),
                    DestinationCityId = table.Column<int>(type: "INTEGER", nullable: true),
                    StartUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TotalTripHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    InvalidReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name",
                table: "Cities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentEvents_EquipmentId_EventUtcTime",
                table: "EquipmentEvents",
                columns: new[] { "EquipmentId", "EventUtcTime" });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentEvents_NaturalKeyHash",
                table: "EquipmentEvents",
                column: "NaturalKeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_EquipmentId_StartUtc",
                table: "Trips",
                columns: new[] { "EquipmentId", "StartUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "EquipmentEvents");

            migrationBuilder.DropTable(
                name: "Trips");
        }
    }
}
