using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace toyiyo.todo.Migrations
{
    public partial class subscription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalSubscriptionId",
                table: "AbpTenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionSeats",
                table: "AbpTenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalSubscriptionId",
                table: "AbpTenants");

            migrationBuilder.DropColumn(
                name: "SubscriptionSeats",
                table: "AbpTenants");
        }
    }
}
