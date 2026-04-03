#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace api_demo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBasketTableAddSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Baskets",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Baskets");
        }
    }
}
