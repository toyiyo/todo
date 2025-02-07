using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace toyiyo.todo.Migrations
{
    public partial class FixJobImageLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobImage_Jobs_JobId",
                table: "JobImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JobImage",
                table: "JobImage");

            migrationBuilder.DropIndex(
                name: "IX_JobImage_JobId",
                table: "JobImage");

            migrationBuilder.RenameTable(
                name: "JobImage",
                newName: "JobImages");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobImages",
                table: "JobImages",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_JobImages",
                table: "JobImages");

            migrationBuilder.RenameTable(
                name: "JobImages",
                newName: "JobImage");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobImage",
                table: "JobImage",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_JobImage_JobId",
                table: "JobImage",
                column: "JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobImage_Jobs_JobId",
                table: "JobImage",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
