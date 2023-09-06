using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace toyiyo.todo.Migrations
{
    public partial class LevelIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jobs_JobStatus",
                table: "Jobs");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobStatus_Level",
                table: "Jobs",
                columns: new[] { "JobStatus", "Level" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jobs_JobStatus_Level",
                table: "Jobs");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobStatus",
                table: "Jobs",
                column: "JobStatus");
        }
    }
}
