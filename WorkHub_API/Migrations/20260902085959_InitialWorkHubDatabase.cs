using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkHub_API.Migrations
{
    /// <inheritdoc />
    public partial class InitialWorkHubDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketplaceCategories",
                columns: table => new
                {
                    MarketplaceCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceCategories", x => x.MarketplaceCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCategories",
                columns: table => new
                {
                    ServiceCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategories", x => x.ServiceCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    SkillId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkillName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.SkillId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AccountStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "CompanyProfiles",
                columns: table => new
                {
                    CompanyProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LogoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerificationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyProfiles", x => x.CompanyProfileId);
                    table.ForeignKey(
                        name: "FK_CompanyProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobSeekerProfiles",
                columns: table => new
                {
                    JobSeekerProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProfessionalSummary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PreferredJobCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpectedSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CurrentLocation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ProfileImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerProfiles", x => x.JobSeekerProfileId);
                    table.ForeignKey(
                        name: "FK_JobSeekerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceSellerProfiles",
                columns: table => new
                {
                    MarketplaceSellerProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    VerificationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsBusinessSeller = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceSellerProfiles", x => x.MarketplaceSellerProfileId);
                    table.ForeignKey(
                        name: "FK_MarketplaceSellerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceProviderProfiles",
                columns: table => new
                {
                    ServiceProviderProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: false),
                    StartingPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    VerificationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProviderProfiles", x => x.ServiceProviderProfileId);
                    table.ForeignKey(
                        name: "FK_ServiceProviderProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    ServiceRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ServiceCategoryId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PreferredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.ServiceRequestId);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_ServiceCategories_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "ServiceCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyProfileId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    JobType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SalaryMin = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SalaryMax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MinimumExperienceYears = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    EducationRequirement = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClosingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.JobId);
                    table.ForeignKey(
                        name: "FK_Jobs_CompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CompanyProfiles",
                        principalColumn: "CompanyProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CVDocuments",
                columns: table => new
                {
                    CVDocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobSeekerProfileId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CVDocuments", x => x.CVDocumentId);
                    table.ForeignKey(
                        name: "FK_CVDocuments_JobSeekerProfiles_JobSeekerProfileId",
                        column: x => x.JobSeekerProfileId,
                        principalTable: "JobSeekerProfiles",
                        principalColumn: "JobSeekerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobSeekerEducations",
                columns: table => new
                {
                    JobSeekerEducationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobSeekerProfileId = table.Column<int>(type: "int", nullable: false),
                    InstitutionName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Qualification = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FieldOfStudy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrentlyStudying = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerEducations", x => x.JobSeekerEducationId);
                    table.ForeignKey(
                        name: "FK_JobSeekerEducations_JobSeekerProfiles_JobSeekerProfileId",
                        column: x => x.JobSeekerProfileId,
                        principalTable: "JobSeekerProfiles",
                        principalColumn: "JobSeekerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobSeekerExperiences",
                columns: table => new
                {
                    JobSeekerExperienceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobSeekerProfileId = table.Column<int>(type: "int", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrentJob = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerExperiences", x => x.JobSeekerExperienceId);
                    table.ForeignKey(
                        name: "FK_JobSeekerExperiences_JobSeekerProfiles_JobSeekerProfileId",
                        column: x => x.JobSeekerProfileId,
                        principalTable: "JobSeekerProfiles",
                        principalColumn: "JobSeekerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobSeekerSkills",
                columns: table => new
                {
                    JobSeekerSkillId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobSeekerProfileId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    ProficiencyLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    YearsOfExperience = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerSkills", x => x.JobSeekerSkillId);
                    table.ForeignKey(
                        name: "FK_JobSeekerSkills_JobSeekerProfiles_JobSeekerProfileId",
                        column: x => x.JobSeekerProfileId,
                        principalTable: "JobSeekerProfiles",
                        principalColumn: "JobSeekerProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobSeekerSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceListings",
                columns: table => new
                {
                    MarketplaceListingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarketplaceSellerProfileId = table.Column<int>(type: "int", nullable: false),
                    MarketplaceCategoryId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ListingType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Condition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceListings", x => x.MarketplaceListingId);
                    table.ForeignKey(
                        name: "FK_MarketplaceListings_MarketplaceCategories_MarketplaceCategoryId",
                        column: x => x.MarketplaceCategoryId,
                        principalTable: "MarketplaceCategories",
                        principalColumn: "MarketplaceCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceListings_MarketplaceSellerProfiles_MarketplaceSellerProfileId",
                        column: x => x.MarketplaceSellerProfileId,
                        principalTable: "MarketplaceSellerProfiles",
                        principalColumn: "MarketplaceSellerProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceOfferings",
                columns: table => new
                {
                    ServiceOfferingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceProviderProfileId = table.Column<int>(type: "int", nullable: false),
                    ServiceCategoryId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StartingPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOfferings", x => x.ServiceOfferingId);
                    table.ForeignKey(
                        name: "FK_ServiceOfferings_ServiceCategories_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "ServiceCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceOfferings_ServiceProviderProfiles_ServiceProviderProfileId",
                        column: x => x.ServiceProviderProfileId,
                        principalTable: "ServiceProviderProfiles",
                        principalColumn: "ServiceProviderProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceReviews",
                columns: table => new
                {
                    ServiceReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceProviderProfileId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceReviews", x => x.ServiceReviewId);
                    table.ForeignKey(
                        name: "FK_ServiceReviews_ServiceProviderProfiles_ServiceProviderProfileId",
                        column: x => x.ServiceProviderProfileId,
                        principalTable: "ServiceProviderProfiles",
                        principalColumn: "ServiceProviderProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceReviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceOffers",
                columns: table => new
                {
                    ServiceOfferId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceRequestId = table.Column<int>(type: "int", nullable: false),
                    ServiceProviderProfileId = table.Column<int>(type: "int", nullable: false),
                    OfferedPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOffers", x => x.ServiceOfferId);
                    table.ForeignKey(
                        name: "FK_ServiceOffers_ServiceProviderProfiles_ServiceProviderProfileId",
                        column: x => x.ServiceProviderProfileId,
                        principalTable: "ServiceProviderProfiles",
                        principalColumn: "ServiceProviderProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceOffers_ServiceRequests_ServiceRequestId",
                        column: x => x.ServiceRequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "ServiceRequestId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobSkills",
                columns: table => new
                {
                    JobSkillId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Importance = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequiredYearsOfExperience = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSkills", x => x.JobSkillId);
                    table.ForeignKey(
                        name: "FK_JobSkills_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobApplications",
                columns: table => new
                {
                    JobApplicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    JobSeekerProfileId = table.Column<int>(type: "int", nullable: false),
                    CVDocumentId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CoverLetter = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplications", x => x.JobApplicationId);
                    table.ForeignKey(
                        name: "FK_JobApplications_CVDocuments_CVDocumentId",
                        column: x => x.CVDocumentId,
                        principalTable: "CVDocuments",
                        principalColumn: "CVDocumentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplications_JobSeekerProfiles_JobSeekerProfileId",
                        column: x => x.JobSeekerProfileId,
                        principalTable: "JobSeekerProfiles",
                        principalColumn: "JobSeekerProfileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplications_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceImages",
                columns: table => new
                {
                    MarketplaceImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarketplaceListingId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceImages", x => x.MarketplaceImageId);
                    table.ForeignKey(
                        name: "FK_MarketplaceImages_MarketplaceListings_MarketplaceListingId",
                        column: x => x.MarketplaceListingId,
                        principalTable: "MarketplaceListings",
                        principalColumn: "MarketplaceListingId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CVMatchResults",
                columns: table => new
                {
                    CVMatchResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobApplicationId = table.Column<int>(type: "int", nullable: false),
                    SkillsScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ExperienceScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    EducationScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PreferredSkillsScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    RelevanceScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    LocationScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    OverallScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MatchedSkills = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    MissingSkills = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CVMatchResults", x => x.CVMatchResultId);
                    table.ForeignKey(
                        name: "FK_CVMatchResults_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "JobApplicationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProfiles_UserId",
                table: "CompanyProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CVDocuments_JobSeekerProfileId",
                table: "CVDocuments",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CVMatchResults_JobApplicationId",
                table: "CVMatchResults",
                column: "JobApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CVDocumentId",
                table: "JobApplications",
                column: "CVDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobId_JobSeekerProfileId",
                table: "JobApplications",
                columns: new[] { "JobId", "JobSeekerProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobSeekerProfileId",
                table: "JobApplications",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CompanyProfileId",
                table: "Jobs",
                column: "CompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerEducations_JobSeekerProfileId",
                table: "JobSeekerEducations",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerExperiences_JobSeekerProfileId",
                table: "JobSeekerExperiences",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerProfiles_UserId",
                table: "JobSeekerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerSkills_JobSeekerProfileId_SkillId",
                table: "JobSeekerSkills",
                columns: new[] { "JobSeekerProfileId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerSkills_SkillId",
                table: "JobSeekerSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSkills_JobId_SkillId",
                table: "JobSkills",
                columns: new[] { "JobId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobSkills_SkillId",
                table: "JobSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceCategories_CategoryName",
                table: "MarketplaceCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceImages_MarketplaceListingId",
                table: "MarketplaceImages",
                column: "MarketplaceListingId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceListings_MarketplaceCategoryId",
                table: "MarketplaceListings",
                column: "MarketplaceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceListings_MarketplaceSellerProfileId",
                table: "MarketplaceListings",
                column: "MarketplaceSellerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceSellerProfiles_UserId",
                table: "MarketplaceSellerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_CategoryName",
                table: "ServiceCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOfferings_ServiceCategoryId",
                table: "ServiceOfferings",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOfferings_ServiceProviderProfileId",
                table: "ServiceOfferings",
                column: "ServiceProviderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOffers_ServiceProviderProfileId",
                table: "ServiceOffers",
                column: "ServiceProviderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOffers_ServiceRequestId",
                table: "ServiceOffers",
                column: "ServiceRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderProfiles_UserId",
                table: "ServiceProviderProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ServiceCategoryId",
                table: "ServiceRequests",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_UserId",
                table: "ServiceRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReviews_ServiceProviderProfileId",
                table: "ServiceReviews",
                column: "ServiceProviderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReviews_UserId",
                table: "ServiceReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_SkillName",
                table: "Skills",
                column: "SkillName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CVMatchResults");

            migrationBuilder.DropTable(
                name: "JobSeekerEducations");

            migrationBuilder.DropTable(
                name: "JobSeekerExperiences");

            migrationBuilder.DropTable(
                name: "JobSeekerSkills");

            migrationBuilder.DropTable(
                name: "JobSkills");

            migrationBuilder.DropTable(
                name: "MarketplaceImages");

            migrationBuilder.DropTable(
                name: "ServiceOfferings");

            migrationBuilder.DropTable(
                name: "ServiceOffers");

            migrationBuilder.DropTable(
                name: "ServiceReviews");

            migrationBuilder.DropTable(
                name: "JobApplications");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "MarketplaceListings");

            migrationBuilder.DropTable(
                name: "ServiceRequests");

            migrationBuilder.DropTable(
                name: "ServiceProviderProfiles");

            migrationBuilder.DropTable(
                name: "CVDocuments");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "MarketplaceCategories");

            migrationBuilder.DropTable(
                name: "MarketplaceSellerProfiles");

            migrationBuilder.DropTable(
                name: "ServiceCategories");

            migrationBuilder.DropTable(
                name: "JobSeekerProfiles");

            migrationBuilder.DropTable(
                name: "CompanyProfiles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
