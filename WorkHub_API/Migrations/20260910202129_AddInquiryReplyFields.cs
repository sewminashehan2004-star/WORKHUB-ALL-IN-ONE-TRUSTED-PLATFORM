using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkHub_API.Migrations
{
    /// <inheritdoc />
    public partial class AddInquiryReplyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RepliedAt",
                table: "Inquiries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepliedBy",
                table: "Inquiries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReplyMessage",
                table: "Inquiries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepliedAt",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "RepliedBy",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "ReplyMessage",
                table: "Inquiries");
        }
    }
}
