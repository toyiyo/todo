using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace toyiyo.todo.Migrations
{
    public partial class IndexTenantOnUserAndRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_TenantId",
                table: "AbpUsers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpRoles_TenantId",
                table: "AbpRoles",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AbpUsers_TenantId",
                table: "AbpUsers");

            migrationBuilder.DropIndex(
                name: "IX_AbpRoles_TenantId",
                table: "AbpRoles");
        }
    }
}
