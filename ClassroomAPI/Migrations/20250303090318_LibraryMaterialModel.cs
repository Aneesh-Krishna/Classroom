using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomAPI.Migrations
{
    /// <inheritdoc />
    public partial class LibraryMaterialModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LibraryMaterials",
                columns: table => new
                {
                    LibraryMaterialUploadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LibraryMaterialUploadName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LibraryMaterialUploadUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploaderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AcceptedOrRejected = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryMaterials", x => x.LibraryMaterialUploadId);
                    table.ForeignKey(
                        name: "FK_LibraryMaterials_AspNetUsers_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryMaterials_UploaderId",
                table: "LibraryMaterials",
                column: "UploaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibraryMaterials");
        }
    }
}
