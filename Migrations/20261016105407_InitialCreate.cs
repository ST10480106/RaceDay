using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(20)", nullable: false),
                    FullName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", nullable: false),
                    Role = table.Column<string>(type: "varchar(20)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                    table.CheckConstraint("CK_User_IdMatchesRole", "([Role] = 'Organiser' AND [UserId] LIKE 'O%') OR ([Role] = 'Participant' AND [UserId] LIKE 'P%')");
                    table.CheckConstraint("CK_User_Role", "[Role] IN ('Organiser', 'Participant')");
                });

            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganiserId = table.Column<string>(type: "varchar(20)", nullable: false),
                    EventName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Location = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    EventType = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "Run")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.EventId);
                    table.CheckConstraint("CK_Event_EventType", "[EventType] IN ('Run', 'Walk', 'Cycle')");
                    table.ForeignKey(
                        name: "FK_Event_User",
                        column: x => x.OrganiserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    CategoryName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Distance = table.Column<decimal>(type: "decimal(6,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_Category_Event",
                        column: x => x.EventId,
                        principalTable: "Event",
                        principalColumn: "EventId");
                });

            migrationBuilder.CreateTable(
                name: "Enrolment",
                columns: table => new
                {
                    EnrolmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipantId = table.Column<string>(type: "varchar(20)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "Pending"),
                    EnrolledTime = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrolment", x => x.EnrolmentId);
                    table.CheckConstraint("CK_Enrolment_Status", "[Status] IN ('Pending', 'Confirmed', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_Enrolment_Category",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "CategoryId");
                    table.ForeignKey(
                        name: "FK_Enrolment_User",
                        column: x => x.ParticipantId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Route",
                columns: table => new
                {
                    RouteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    MapFileUrl = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Route", x => x.RouteId);
                    table.ForeignKey(
                        name: "FK_Route_Category",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "CategoryId");
                });

            migrationBuilder.CreateTable(
                name: "Result",
                columns: table => new
                {
                    ResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrolmentId = table.Column<int>(type: "int", nullable: false),
                    CapturedBy = table.Column<string>(type: "varchar(20)", nullable: false),
                    FinishTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Position = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Result", x => x.ResultId);
                    table.ForeignKey(
                        name: "FK_Result_Enrolment",
                        column: x => x.EnrolmentId,
                        principalTable: "Enrolment",
                        principalColumn: "EnrolmentId");
                    table.ForeignKey(
                        name: "FK_Result_User",
                        column: x => x.CapturedBy,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Category_EventId",
                table: "Category",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolment_CategoryId",
                table: "Enrolment",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "UQ_Enrolment_Participant_Category",
                table: "Enrolment",
                columns: new[] { "ParticipantId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Event_OrganiserId",
                table: "Event",
                column: "OrganiserId");

            migrationBuilder.CreateIndex(
                name: "IX_Result_CapturedBy",
                table: "Result",
                column: "CapturedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Result_EnrolmentId",
                table: "Result",
                column: "EnrolmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Route_CategoryId",
                table: "Route",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Result");

            migrationBuilder.DropTable(
                name: "Route");

            migrationBuilder.DropTable(
                name: "Enrolment");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Event");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
