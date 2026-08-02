using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAppV3.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClassSessionRecurrenceGroupId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecurrenceGroupId",
                schema: "dbo",
                table: "ClassSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassSession_RecurrenceGroupId",
                schema: "dbo",
                table: "ClassSessions",
                column: "RecurrenceGroupId",
                filter: "[RecurrenceGroupId] IS NOT NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClassSession_RecurrenceGroupId",
                schema: "dbo",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "RecurrenceGroupId",
                schema: "dbo",
                table: "ClassSessions");
        }
    }
}
