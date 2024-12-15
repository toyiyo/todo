using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace toyiyo.todo.Migrations
{
    public partial class IndexOnJob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_TenantId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_JobStatus_Level",
                table: "Jobs");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TenantId_Id_IsDeleted",
                table: "Projects",
                columns: new[] { "TenantId", "Id", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobStatus_Level_TenantId",
                table: "Jobs",
                columns: new[] { "JobStatus", "Level", "TenantId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_TenantId_Id_IsDeleted",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_JobStatus_Level_TenantId",
                table: "Jobs");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TenantId",
                table: "Projects",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobStatus_Level",
                table: "Jobs",
                columns: new[] { "JobStatus", "Level" });
        }
    }
}
