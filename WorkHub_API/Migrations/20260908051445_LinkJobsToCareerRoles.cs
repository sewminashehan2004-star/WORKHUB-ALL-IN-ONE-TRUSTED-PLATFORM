using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkHub_API.Migrations
{
    /// <inheritdoc />
    public partial class LinkJobsToCareerRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CareerRoleId",
                table: "Jobs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CareerRoleId",
                table: "Jobs",
                column: "CareerRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_CareerRoles_CareerRoleId",
                table: "Jobs",
                column: "CareerRoleId",
                principalTable: "CareerRoles",
                principalColumn: "CareerRoleId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_CareerRoles_CareerRoleId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_CareerRoleId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CareerRoleId",
                table: "Jobs");
        }
    }
}
