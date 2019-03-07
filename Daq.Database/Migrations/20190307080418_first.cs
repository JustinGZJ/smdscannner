using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Daq.Database.Migrations
{
    public partial class first : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlarmInfos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StationId = table.Column<int>(nullable: false),
                    AlarmIndex = table.Column<int>(nullable: false),
                    AlarmContent = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alarms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Time = table.Column<DateTime>(nullable: false),
                    AlarmInfoId = table.Column<int>(nullable: false),
                    StatusInfoId = table.Column<int>(nullable: true),
                    Span = table.Column<TimeSpan>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alarms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alarms_AlarmInfos_StatusInfoId",
                        column: x => x.StatusInfoId,
                        principalTable: "AlarmInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_StatusInfoId",
                table: "Alarms",
                column: "StatusInfoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alarms");

            migrationBuilder.DropTable(
                name: "AlarmInfos");
        }
    }
}
