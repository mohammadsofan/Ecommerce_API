using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mshop.Api.Migrations
{
    /// <inheritdoc />
    public partial class addIsUsedcolumntoResetPasswordCodemodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "ResetPasswordCode",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "ResetPasswordCode");
        }
    }
}
