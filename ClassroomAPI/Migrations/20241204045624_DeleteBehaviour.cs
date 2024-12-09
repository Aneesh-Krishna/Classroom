using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomAPI.Migrations
{
    /// <inheritdoc />
    public partial class DeleteBehaviour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentSubmissions_AspNetUsers_SubmittedById",
                table: "AssignmentSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Chats_AspNetUsers_UserId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseMembers_AspNetUsers_UserId",
                table: "CourseMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseMembers_Courses_CourseId",
                table: "CourseMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_AspNetUsers_AdminId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizResponses_AspNetUsers_UserId",
                table: "QuizResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Courses_CourseId",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_AssignmentSubmissions_SubmittedById",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "SubmittedById",
                table: "AssignmentSubmissions");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AssignmentSubmissions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_UserId",
                table: "AssignmentSubmissions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentSubmissions_AspNetUsers_UserId",
                table: "AssignmentSubmissions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_AspNetUsers_UserId",
                table: "Chats",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseMembers_AspNetUsers_UserId",
                table: "CourseMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseMembers_Courses_CourseId",
                table: "CourseMembers",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_AspNetUsers_AdminId",
                table: "Courses",
                column: "AdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizResponses_AspNetUsers_UserId",
                table: "QuizResponses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Courses_CourseId",
                table: "Quizzes",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentSubmissions_AspNetUsers_UserId",
                table: "AssignmentSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Chats_AspNetUsers_UserId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseMembers_AspNetUsers_UserId",
                table: "CourseMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseMembers_Courses_CourseId",
                table: "CourseMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_AspNetUsers_AdminId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizResponses_AspNetUsers_UserId",
                table: "QuizResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Courses_CourseId",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_AssignmentSubmissions_UserId",
                table: "AssignmentSubmissions");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AssignmentSubmissions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "SubmittedById",
                table: "AssignmentSubmissions",
                type: "nvarchar(450)",
                nullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_AspNetUsers_UserId",
                table: "Chats",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseMembers_AspNetUsers_UserId",
                table: "CourseMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseMembers_Courses_CourseId",
                table: "CourseMembers",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_AspNetUsers_AdminId",
                table: "Courses",
                column: "AdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizResponses_AspNetUsers_UserId",
                table: "QuizResponses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Courses_CourseId",
                table: "Quizzes",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId");
        }
    }
}
