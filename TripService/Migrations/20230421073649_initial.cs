using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripService.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trip",
                columns: table => new
                {
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: true),
                    PassengerId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompleteTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Destination = table.Column<string>(type: "text", nullable: true),
                    PassengerPhone = table.Column<string>(type: "text", nullable: true),
                    LatDesAddr = table.Column<double>(type: "double precision", nullable: true),
                    LongDesAddr = table.Column<double>(type: "double precision", nullable: true),
                    StartAddress = table.Column<string>(type: "text", nullable: true),
                    LatStartAddr = table.Column<double>(type: "double precision", nullable: true),
                    LongStartAddr = table.Column<double>(type: "double precision", nullable: true),
                    TripStatus = table.Column<string>(type: "text", nullable: true),
                    Distance = table.Column<double>(type: "double precision", nullable: true),
                    Price = table.Column<int>(type: "integer", nullable: true),
                    VehicleType = table.Column<string>(type: "text", nullable: true),
                    TimeSecond = table.Column<int>(type: "integer", nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trip", x => x.TripId);
                });

            migrationBuilder.CreateTable(
                name: "TripFeedback",
                columns: table => new
                {
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Rate = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripFeedback", x => x.TripId);
                });

            migrationBuilder.CreateTable(
                name: "TripRequest",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    PassengerId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestStatus = table.Column<string>(type: "text", nullable: true),
                    Destination = table.Column<string>(type: "text", nullable: true),
                    LatDesAddr = table.Column<double>(type: "double precision", nullable: true),
                    LongDesAddr = table.Column<double>(type: "double precision", nullable: true),
                    StartAddress = table.Column<string>(type: "text", nullable: true),
                    LatStartAddr = table.Column<double>(type: "double precision", nullable: true),
                    LongStartAddr = table.Column<double>(type: "double precision", nullable: true),
                    PassengerPhone = table.Column<string>(type: "text", nullable: true),
                    PassengerNote = table.Column<string>(type: "text", nullable: true),
                    Distance = table.Column<double>(type: "double precision", nullable: true),
                    VehicleType = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<int>(type: "integer", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripRequest", x => x.RequestId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trip");

            migrationBuilder.DropTable(
                name: "TripFeedback");

            migrationBuilder.DropTable(
                name: "TripRequest");
        }
    }
}
