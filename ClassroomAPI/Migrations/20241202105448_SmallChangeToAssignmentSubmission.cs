using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomAPI.Migrations
{
    /// <inheritdoc />
    public partial class SmallChangeToAssignmentSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubmittedById",
                table: "AssignmentSubmissions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "AssignmentSubmissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_SubmittedById",
                table: "AssignmentSubmissions",
                column: "SubmittedById");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentSubmissions_AspNetUsers_SubmittedById",
                table: "AssignmentSubmissions",
                column: "SubmittedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentSubmissions_AspNetUsers_SubmittedById",
                table: "AssignmentSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_AssignmentSubmissions_SubmittedById",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "SubmittedById",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AssignmentSubmissions");
        }
    }
}
