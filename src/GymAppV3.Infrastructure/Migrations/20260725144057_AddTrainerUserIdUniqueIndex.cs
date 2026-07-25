using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAppV3.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerUserIdUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Trainers_UserId",
                schema: "dbo",
                table: "Trainers",
                column: "UserId",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trainers_UserId",
                schema: "dbo",
                table: "Trainers");
        }
    }
}
