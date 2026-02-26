using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastGooey.Migrations
{
    /// <inheritdoc />
    public partial class BackfillWorkspaceOwnersFromLegacyWorkspaceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE workspaces AS workspace_row
                SET owner_user_id = legacy_user.id
                FROM LATERAL (
                    SELECT user_row.id
                    FROM "AspNetUsers" AS user_row
                    WHERE user_row.workspace_id = workspace_row.id
                    ORDER BY user_row.id
                    LIMIT 1
                ) AS legacy_user
                WHERE workspace_row.owner_user_id IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally no-op: previous owner_user_id values cannot be reconstructed safely.
        }
    }
}
