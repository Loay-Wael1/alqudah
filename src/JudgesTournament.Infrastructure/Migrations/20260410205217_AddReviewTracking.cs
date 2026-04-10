using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JudgesTournament.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                table: "TeamRegistrations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedByAdminId",
                table: "TeamRegistrations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                table: "TeamRegistrations");

            migrationBuilder.DropColumn(
                name: "ReviewedByAdminId",
                table: "TeamRegistrations");
        }
    }
}
