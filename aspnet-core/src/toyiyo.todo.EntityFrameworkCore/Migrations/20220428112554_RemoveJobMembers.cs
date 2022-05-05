using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace toyiyo.todo.Migrations
{
    public partial class RemoveJobMembers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpUsers_Jobs_JobId",
                table: "AbpUsers");

            migrationBuilder.DropIndex(
                name: "IX_AbpUsers_JobId",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "AbpUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "AbpUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_JobId",
                table: "AbpUsers",
                column: "JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUsers_Jobs_JobId",
                table: "AbpUsers",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id");
        }
    }
}
