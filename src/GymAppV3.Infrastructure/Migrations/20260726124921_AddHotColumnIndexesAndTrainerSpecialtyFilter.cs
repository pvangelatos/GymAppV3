using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAppV3.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHotColumnIndexesAndTrainerSpecialtyFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_MemberId",
                schema: "dbo",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_MemberId",
                schema: "dbo",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_ClassSessions_ClassRoomId",
                schema: "dbo",
                table: "ClassSessions");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_MemberId",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MemberId_PaidAt",
                schema: "dbo",
                table: "Payments",
                columns: new[] { "MemberId", "PaidAt" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_MemberId_EndDate",
                schema: "dbo",
                table: "Memberships",
                columns: new[] { "MemberId", "EndDate" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSession_ClassRoomId_StartsAt",
                schema: "dbo",
                table: "ClassSessions",
                columns: new[] { "ClassRoomId", "StartsAt" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSession_StartsAt",
                schema: "dbo",
                table: "ClassSessions",
                column: "StartsAt",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_MemberId_BookedAt",
                schema: "dbo",
                table: "Bookings",
                columns: new[] { "MemberId", "BookedAt" },
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_MemberId_PaidAt",
                schema: "dbo",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_MemberId_EndDate",
                schema: "dbo",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_ClassSession_ClassRoomId_StartsAt",
                schema: "dbo",
                table: "ClassSessions");

            migrationBuilder.DropIndex(
                name: "IX_ClassSession_StartsAt",
                schema: "dbo",
                table: "ClassSessions");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_MemberId_BookedAt",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MemberId",
                schema: "dbo",
                table: "Payments",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_MemberId",
                schema: "dbo",
                table: "Memberships",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_ClassRoomId",
                schema: "dbo",
                table: "ClassSessions",
                column: "ClassRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_MemberId",
                schema: "dbo",
                table: "Bookings",
                column: "MemberId");
        }
    }
}
