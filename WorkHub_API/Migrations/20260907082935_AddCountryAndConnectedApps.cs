using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkHub_API.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryAndConnectedApps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "UserPublicProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacebookUrl",
                table: "UserPublicProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHubUrl",
                table: "UserPublicProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramUrl",
                table: "UserPublicProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInUrl",
                table: "UserPublicProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "UserPublicProfiles");

            migrationBuilder.DropColumn(
                name: "FacebookUrl",
                table: "UserPublicProfiles");

            migrationBuilder.DropColumn(
                name: "GitHubUrl",
                table: "UserPublicProfiles");

            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "UserPublicProfiles");

            migrationBuilder.DropColumn(
                name: "LinkedInUrl",
                table: "UserPublicProfiles");
        }
    }
}
