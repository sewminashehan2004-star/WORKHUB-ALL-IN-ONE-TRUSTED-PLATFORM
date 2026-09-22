using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkHub_API.Migrations
{
    /// <inheritdoc />
    public partial class SyncUserProfileChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "UserPublicProfiles",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "UserPublicProfiles");
        }
    }
}
