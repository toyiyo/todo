using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace toyiyo.todo.Migrations
{
    public partial class Jobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "AbpUsers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true),
                    AssigneeId = table.Column<long>(type: "bigint", nullable: true),
                    JobStatus = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_AbpUsers_AssigneeId",
                        column: x => x.AssigneeId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Jobs_AbpUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_JobId",
                table: "AbpUsers",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_AssigneeId",
                table: "Jobs",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OwnerId",
                table: "Jobs",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbpUsers_Jobs_JobId",
                table: "AbpUsers",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbpUsers_Jobs_JobId",
                table: "AbpUsers");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_AbpUsers_JobId",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "AbpUsers");
        }
    }
}
