using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomAPI.Migrations
{
    /// <inheritdoc />
    public partial class ParticipantForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_AspNetUsers_ApplicationUserId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_ApplicationUserId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Meetings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Meetings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_ApplicationUserId",
                table: "Meetings",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_AspNetUsers_ApplicationUserId",
                table: "Meetings",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
