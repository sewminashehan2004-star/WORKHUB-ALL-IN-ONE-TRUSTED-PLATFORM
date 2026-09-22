using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkHub_API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPublicProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPublicProfiles",
                columns: table => new
                {
                    UserPublicProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Headline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    About = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfileImageFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImageFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPublicProfiles", x => x.UserPublicProfileId);
                    table.ForeignKey(
                        name: "FK_UserPublicProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPublicProfileEducations",
                columns: table => new
                {
                    UserPublicProfileEducationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserPublicProfileId = table.Column<int>(type: "int", nullable: false),
                    Institution = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Qualification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FieldOfStudy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartYear = table.Column<int>(type: "int", nullable: true),
                    EndYear = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPublicProfileEducations", x => x.UserPublicProfileEducationId);
                    table.ForeignKey(
                        name: "FK_UserPublicProfileEducations_UserPublicProfiles_UserPublicProfileId",
                        column: x => x.UserPublicProfileId,
                        principalTable: "UserPublicProfiles",
                        principalColumn: "UserPublicProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPublicProfileSkills",
                columns: table => new
                {
                    UserPublicProfileSkillId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserPublicProfileId = table.Column<int>(type: "int", nullable: false),
                    SkillName = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPublicProfileSkills", x => x.UserPublicProfileSkillId);
                    table.ForeignKey(
                        name: "FK_UserPublicProfileSkills_UserPublicProfiles_UserPublicProfileId",
                        column: x => x.UserPublicProfileId,
                        principalTable: "UserPublicProfiles",
                        principalColumn: "UserPublicProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicProfileEducations_UserPublicProfileId",
                table: "UserPublicProfileEducations",
                column: "UserPublicProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicProfiles_UserId",
                table: "UserPublicProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicProfileSkills_UserPublicProfileId_SkillName",
                table: "UserPublicProfileSkills",
                columns: new[] { "UserPublicProfileId", "SkillName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPublicProfileEducations");

            migrationBuilder.DropTable(
                name: "UserPublicProfileSkills");

            migrationBuilder.DropTable(
                name: "UserPublicProfiles");
        }
    }
}
