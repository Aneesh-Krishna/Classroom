using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomAPI.Migrations
{
    /// <inheritdoc />
    public partial class MeetingHasEnded_ParticipantHasLeft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "hasLeft",
                table: "Participants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "hasEnded",
                table: "Meetings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hasLeft",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "hasEnded",
                table: "Meetings");
        }
    }
}
