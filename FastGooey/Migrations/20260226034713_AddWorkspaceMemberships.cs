using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FastGooey.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workspace_memberships",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    workspace_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workspace_memberships", x => x.id);
                    table.ForeignKey(
                        name: "fk_workspace_memberships_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_workspace_memberships_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_workspace_memberships_user_id",
                table: "workspace_memberships",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_workspace_memberships_user_id_workspace_id",
                table: "workspace_memberships",
                columns: new[] { "user_id", "workspace_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workspace_memberships_workspace_id",
                table: "workspace_memberships",
                column: "workspace_id");

            migrationBuilder.Sql("""
                INSERT INTO workspace_memberships (user_id, workspace_id)
                SELECT DISTINCT user_row.id, user_row.workspace_id
                FROM "AspNetUsers" AS user_row
                WHERE user_row.workspace_id IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM workspace_memberships AS membership
                      WHERE membership.user_id = user_row.id
                        AND membership.workspace_id = user_row.workspace_id
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workspace_memberships");
        }
    }
}
