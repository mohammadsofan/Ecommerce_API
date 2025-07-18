using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mshop.Api.Migrations
{
    /// <inheritdoc />
    public partial class addcreateddatetoreviewmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResetPasswordCode_AspNetUsers_ApplicationUserId",
                table: "ResetPasswordCode");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_AspNetUsers_ApplicationUserId",
                table: "Review");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Products_ProductId",
                table: "Review");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Review",
                table: "Review");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResetPasswordCode",
                table: "ResetPasswordCode");

            migrationBuilder.RenameTable(
                name: "Review",
                newName: "Reviews");

            migrationBuilder.RenameTable(
                name: "ResetPasswordCode",
                newName: "ResetPasswordCodes");

            migrationBuilder.RenameIndex(
                name: "IX_Review_ApplicationUserId",
                table: "Reviews",
                newName: "IX_Reviews_ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ResetPasswordCode_ApplicationUserId",
                table: "ResetPasswordCodes",
                newName: "IX_ResetPasswordCodes_ApplicationUserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Reviews",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reviews",
                table: "Reviews",
                columns: new[] { "ProductId", "ApplicationUserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResetPasswordCodes",
                table: "ResetPasswordCodes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ResetPasswordCodes_AspNetUsers_ApplicationUserId",
                table: "ResetPasswordCodes",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AspNetUsers_ApplicationUserId",
                table: "Reviews",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Products_ProductId",
                table: "Reviews",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResetPasswordCodes_AspNetUsers_ApplicationUserId",
                table: "ResetPasswordCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AspNetUsers_ApplicationUserId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Products_ProductId",
                table: "Reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reviews",
                table: "Reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResetPasswordCodes",
                table: "ResetPasswordCodes");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Reviews");

            migrationBuilder.RenameTable(
                name: "Reviews",
                newName: "Review");

            migrationBuilder.RenameTable(
                name: "ResetPasswordCodes",
                newName: "ResetPasswordCode");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_ApplicationUserId",
                table: "Review",
                newName: "IX_Review_ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ResetPasswordCodes_ApplicationUserId",
                table: "ResetPasswordCode",
                newName: "IX_ResetPasswordCode_ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Review",
                table: "Review",
                columns: new[] { "ProductId", "ApplicationUserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResetPasswordCode",
                table: "ResetPasswordCode",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ResetPasswordCode_AspNetUsers_ApplicationUserId",
                table: "ResetPasswordCode",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_AspNetUsers_ApplicationUserId",
                table: "Review",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Products_ProductId",
                table: "Review",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
