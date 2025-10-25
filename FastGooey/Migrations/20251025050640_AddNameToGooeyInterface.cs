using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastGooey.Migrations
{
    /// <inheritdoc />
    public partial class AddNameToGooeyInterface : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "gooey_interfaces",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name",
                table: "gooey_interfaces");
        }
    }
}
