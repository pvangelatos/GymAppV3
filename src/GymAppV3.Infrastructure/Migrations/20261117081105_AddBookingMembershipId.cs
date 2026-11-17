using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAppV3.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingMembershipId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MembershipId",
                schema: "dbo",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_MembershipId",
                schema: "dbo",
                table: "Bookings",
                column: "MembershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Memberships_MembershipId",
                schema: "dbo",
                table: "Bookings",
                column: "MembershipId",
                principalSchema: "dbo",
                principalTable: "Memberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Memberships_MembershipId",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_MembershipId",
                schema: "dbo",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "MembershipId",
                schema: "dbo",
                table: "Bookings");
        }
    }
}
