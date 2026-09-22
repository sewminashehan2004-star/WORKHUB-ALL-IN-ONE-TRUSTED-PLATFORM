using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkHub_API.Migrations
{
    /// <inheritdoc />
    public partial class AddCareerIntelligenceFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CareerCategories",
                columns: table => new
                {
                    CareerCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerCategories", x => x.CareerCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "CareerRoles",
                columns: table => new
                {
                    CareerRoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CareerCategoryId = table.Column<int>(type: "int", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    BaselineExperienceYears = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    BaselineEducationRequirement = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerRoles", x => x.CareerRoleId);
                    table.ForeignKey(
                        name: "FK_CareerRoles_CareerCategories_CareerCategoryId",
                        column: x => x.CareerCategoryId,
                        principalTable: "CareerCategories",
                        principalColumn: "CareerCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CareerRoleSkills",
                columns: table => new
                {
                    CareerRoleSkillId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CareerRoleId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Importance = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RecommendedYearsOfExperience = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerRoleSkills", x => x.CareerRoleSkillId);
                    table.ForeignKey(
                        name: "FK_CareerRoleSkills_CareerRoles_CareerRoleId",
                        column: x => x.CareerRoleId,
                        principalTable: "CareerRoles",
                        principalColumn: "CareerRoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CareerRoleSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CareerCategories_CategoryName",
                table: "CareerCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CareerRoles_CareerCategoryId_RoleName",
                table: "CareerRoles",
                columns: new[] { "CareerCategoryId", "RoleName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CareerRoleSkills_CareerRoleId_SkillId",
                table: "CareerRoleSkills",
                columns: new[] { "CareerRoleId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CareerRoleSkills_SkillId",
                table: "CareerRoleSkills",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CareerRoleSkills");

            migrationBuilder.DropTable(
                name: "CareerRoles");

            migrationBuilder.DropTable(
                name: "CareerCategories");
        }
    }
}
