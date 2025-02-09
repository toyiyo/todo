using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace toyiyo.todo.Migrations
{
    public partial class adddependencieslink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "Jobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Jobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobId",
                table: "Jobs",
                column: "JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Jobs_JobId",
                table: "Jobs",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Jobs_JobId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_JobId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Jobs");
        }
    }
}
