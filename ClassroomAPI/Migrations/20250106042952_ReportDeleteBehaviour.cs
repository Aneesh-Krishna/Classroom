using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomAPI.Migrations
{
    /// <inheritdoc />
    public partial class ReportDeleteBehaviour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Quizzes_QuizId",
                table: "Reports");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Quizzes_QuizId",
                table: "Reports",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "QuizId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Quizzes_QuizId",
                table: "Reports");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Quizzes_QuizId",
                table: "Reports",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "QuizId");
        }
    }
}
