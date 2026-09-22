using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkHub_API.Migrations
{
    /// <inheritdoc />
    public partial class SyncCurrentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CVAnalysisPurchases",
                columns: table => new
                {
                    CVAnalysisPurchaseId = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    UserId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    JobId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    CVDocumentId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    PlanCode = table.Column<string>(
                        type: "nvarchar(30)",
                        maxLength: 30,
                        nullable: false),

                    Amount = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),

                    Currency = table.Column<string>(
                        type: "nvarchar(20)",
                        maxLength: 20,
                        nullable: false),

                    PaymentStatus = table.Column<string>(
                        type: "nvarchar(30)",
                        maxLength: 30,
                        nullable: false),

                    PaymentReference = table.Column<string>(
                        type: "nvarchar(80)",
                        maxLength: 80,
                        nullable: true),

                    PaymentMethod = table.Column<string>(
                        type: "nvarchar(30)",
                        maxLength: 30,
                        nullable: true),

                    CardLast4 = table.Column<string>(
                        type: "nvarchar(4)",
                        maxLength: 4,
                        nullable: true),

                    MatchPercentage = table.Column<decimal>(
                        type: "decimal(5,2)",
                        precision: 5,
                        scale: 2,
                        nullable: false),

                    MissingSkills = table.Column<string>(
                        type: "nvarchar(4000)",
                        maxLength: 4000,
                        nullable: true),

                    Recommendations = table.Column<string>(
                        type: "nvarchar(4000)",
                        maxLength: 4000,
                        nullable: true),

                    PurchasedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_CVAnalysisPurchases",
                        x => x.CVAnalysisPurchaseId);

                    table.ForeignKey(
                        name: "FK_CVAnalysisPurchases_CVDocuments_CVDocumentId",
                        column: x => x.CVDocumentId,
                        principalTable: "CVDocuments",
                        principalColumn: "CVDocumentId",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_CVAnalysisPurchases_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_CVAnalysisPurchases_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobScreeningQuestions",
                columns: table => new
                {
                    JobScreeningQuestionId = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    JobId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    QuestionText = table.Column<string>(
                        type: "nvarchar(500)",
                        maxLength: 500,
                        nullable: false),

                    QuestionType = table.Column<string>(
                        type: "nvarchar(30)",
                        maxLength: 30,
                        nullable: false),

                    OptionsJson = table.Column<string>(
                        type: "nvarchar(1500)",
                        maxLength: 1500,
                        nullable: true),

                    IsRequired = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    DisplayOrder = table.Column<int>(
                        type: "int",
                        nullable: false),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_JobScreeningQuestions",
                        x => x.JobScreeningQuestionId);

                    table.ForeignKey(
                        name: "FK_JobScreeningQuestions_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceConversations",
                columns: table => new
                {
                    MarketplaceConversationId = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    MarketplaceListingId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    BuyerUserId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    SellerUserId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    Status = table.Column<string>(
                        type: "nvarchar(20)",
                        maxLength: 20,
                        nullable: false),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    UpdatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_MarketplaceConversations",
                        x => x.MarketplaceConversationId);

                    table.ForeignKey(
                        name: "FK_MarketplaceConversations_MarketplaceListings_MarketplaceListingId",
                        column: x => x.MarketplaceListingId,
                        principalTable: "MarketplaceListings",
                        principalColumn: "MarketplaceListingId",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_MarketplaceConversations_Users_BuyerUserId",
                        column: x => x.BuyerUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_MarketplaceConversations_Users_SellerUserId",
                        column: x => x.SellerUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    UserNotificationId = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    UserId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    Title = table.Column<string>(
                        type: "nvarchar(160)",
                        maxLength: 160,
                        nullable: false),

                    Message = table.Column<string>(
                        type: "nvarchar(1500)",
                        maxLength: 1500,
                        nullable: false),

                    NotificationType = table.Column<string>(
                        type: "nvarchar(60)",
                        maxLength: 60,
                        nullable: false),

                    RelatedInquiryId = table.Column<int>(
                        type: "int",
                        nullable: true),

                    IsRead = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_UserNotifications",
                        x => x.UserNotificationId);

                    table.ForeignKey(
                        name: "FK_UserNotifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobScreeningAnswers",
                columns: table => new
                {
                    JobScreeningAnswerId = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    JobApplicationId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    JobScreeningQuestionId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    AnswerText = table.Column<string>(
                        type: "nvarchar(2000)",
                        maxLength: 2000,
                        nullable: false),

                    SubmittedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_JobScreeningAnswers",
                        x => x.JobScreeningAnswerId);

                    table.ForeignKey(
                        name: "FK_JobScreeningAnswers_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "JobApplicationId",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_JobScreeningAnswers_JobScreeningQuestions_JobScreeningQuestionId",
                        column: x => x.JobScreeningQuestionId,
                        principalTable: "JobScreeningQuestions",
                        principalColumn: "JobScreeningQuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceMessages",
                columns: table => new
                {
                    MarketplaceMessageId = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    MarketplaceConversationId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    SenderUserId = table.Column<int>(
                        type: "int",
                        nullable: false),

                    MessageText = table.Column<string>(
                        type: "nvarchar(2000)",
                        maxLength: 2000,
                        nullable: false),

                    SentAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    ReadAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_MarketplaceMessages",
                        x => x.MarketplaceMessageId);

                    table.ForeignKey(
                        name: "FK_MarketplaceMessages_MarketplaceConversations_MarketplaceConversationId",
                        column: x => x.MarketplaceConversationId,
                        principalTable: "MarketplaceConversations",
                        principalColumn: "MarketplaceConversationId",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_MarketplaceMessages_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CVAnalysisPurchases_CVDocumentId",
                table: "CVAnalysisPurchases",
                column: "CVDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_CVAnalysisPurchases_JobId",
                table: "CVAnalysisPurchases",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_CVAnalysisPurchases_UserId",
                table: "CVAnalysisPurchases",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobScreeningAnswers_JobApplicationId_JobScreeningQuestionId",
                table: "JobScreeningAnswers",
                columns: new[]
                {
                    "JobApplicationId",
                    "JobScreeningQuestionId"
                },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobScreeningAnswers_JobScreeningQuestionId",
                table: "JobScreeningAnswers",
                column: "JobScreeningQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_JobScreeningQuestions_JobId_DisplayOrder",
                table: "JobScreeningQuestions",
                columns: new[]
                {
                    "JobId",
                    "DisplayOrder"
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceConversations_BuyerUserId",
                table: "MarketplaceConversations",
                column: "BuyerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceConversations_MarketplaceListingId_BuyerUserId",
                table: "MarketplaceConversations",
                columns: new[]
                {
                    "MarketplaceListingId",
                    "BuyerUserId"
                },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceConversations_SellerUserId",
                table: "MarketplaceConversations",
                column: "SellerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceMessages_MarketplaceConversationId",
                table: "MarketplaceMessages",
                column: "MarketplaceConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceMessages_SenderUserId",
                table: "MarketplaceMessages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId_IsRead_CreatedAt",
                table: "UserNotifications",
                columns: new[]
                {
                    "UserId",
                    "IsRead",
                    "CreatedAt"
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CVAnalysisPurchases");

            migrationBuilder.DropTable(
                name: "JobScreeningAnswers");

            migrationBuilder.DropTable(
                name: "MarketplaceMessages");

            migrationBuilder.DropTable(
                name: "UserNotifications");

            migrationBuilder.DropTable(
                name: "JobScreeningQuestions");

            migrationBuilder.DropTable(
                name: "MarketplaceConversations");
        }
    }
}