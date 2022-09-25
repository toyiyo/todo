using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace toyiyo.todo.Migrations
{
    public partial class IndexTenantOnProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Projects_TenantId",
                table: "Projects",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_TenantId",
                table: "Projects");
        }
    }
}
