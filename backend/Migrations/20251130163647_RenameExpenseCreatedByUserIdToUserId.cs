using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoalboundFamily.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameExpenseCreatedByUserIdToUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Users_CreatedByUserId",
                table: "Expenses");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "Expenses",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_CreatedByUserId",
                table: "Expenses",
                newName: "IX_Expenses_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Users_UserId",
                table: "Expenses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Users_UserId",
                table: "Expenses");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Expenses",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_UserId",
                table: "Expenses",
                newName: "IX_Expenses_CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Users_CreatedByUserId",
                table: "Expenses",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
