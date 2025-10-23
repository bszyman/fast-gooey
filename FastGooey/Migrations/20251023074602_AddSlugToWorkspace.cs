using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastGooey.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugToWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "slug",
                table: "workspaces",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_slug",
                table: "workspaces",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_workspaces_slug",
                table: "workspaces");

            migrationBuilder.DropColumn(
                name: "slug",
                table: "workspaces");
        }
    }
}
