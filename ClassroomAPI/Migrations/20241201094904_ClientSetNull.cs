using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomAPI.Migrations
{
    /// <inheritdoc />
    public partial class ClientSetNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseMembers_Courses_CourseId",
                table: "CourseMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseMembers_Courses_CourseId",
                table: "CourseMembers",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseMembers_Courses_CourseId",
                table: "CourseMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseMembers_Courses_CourseId",
                table: "CourseMembers",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
