using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace toyiyo.todo.Migrations
{
    public partial class RemoveActiveFlagFromInvitation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserInvitations_IsActive",
                table: "UserInvitations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserInvitations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserInvitations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserInvitations_IsActive",
                table: "UserInvitations",
                column: "IsActive");
        }
    }
}
