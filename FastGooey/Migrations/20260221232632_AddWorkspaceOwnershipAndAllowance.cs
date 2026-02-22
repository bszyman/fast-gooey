using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastGooey.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceOwnershipAndAllowance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_explorer",
                table: "workspaces",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "owner_user_id",
                table: "workspaces",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "standard_workspace_allowance",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_owner_user_id",
                table: "workspaces",
                column: "owner_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_workspaces_asp_net_users_owner_user_id",
                table: "workspaces",
                column: "owner_user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_workspaces_asp_net_users_owner_user_id",
                table: "workspaces");

            migrationBuilder.DropIndex(
                name: "ix_workspaces_owner_user_id",
                table: "workspaces");

            migrationBuilder.DropColumn(
                name: "is_explorer",
                table: "workspaces");

            migrationBuilder.DropColumn(
                name: "owner_user_id",
                table: "workspaces");

            migrationBuilder.DropColumn(
                name: "standard_workspace_allowance",
                table: "AspNetUsers");
        }
    }
}
