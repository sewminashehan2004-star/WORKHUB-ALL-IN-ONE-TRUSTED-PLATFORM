using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkHub_API.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceRequestReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceRequestId",
                table: "ServiceReviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReviews_ServiceRequestId",
                table: "ServiceReviews",
                column: "ServiceRequestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceReviews_ServiceRequests_ServiceRequestId",
                table: "ServiceReviews",
                column: "ServiceRequestId",
                principalTable: "ServiceRequests",
                principalColumn: "ServiceRequestId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceReviews_ServiceRequests_ServiceRequestId",
                table: "ServiceReviews");

            migrationBuilder.DropIndex(
                name: "IX_ServiceReviews_ServiceRequestId",
                table: "ServiceReviews");

            migrationBuilder.DropColumn(
                name: "ServiceRequestId",
                table: "ServiceReviews");
        }
    }
}
