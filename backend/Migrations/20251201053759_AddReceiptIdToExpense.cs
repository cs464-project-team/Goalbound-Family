using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoalboundFamily.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptIdToExpense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReceiptId",
                table: "Expenses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ReceiptId",
                table: "Expenses",
                column: "ReceiptId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Receipts_ReceiptId",
                table: "Expenses",
                column: "ReceiptId",
                principalTable: "Receipts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Receipts_ReceiptId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ReceiptId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ReceiptId",
                table: "Expenses");
        }
    }
}
