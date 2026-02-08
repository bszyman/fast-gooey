using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FastGooey.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "media_sources",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    source_type = table.Column<int>(type: "integer", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    workspace_id = table.Column<long>(type: "bigint", nullable: false),
                    s3bucket_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    s3region = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    s3service_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    s3access_key_id = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    s3secret_access_key = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    azure_connection_string = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    azure_container_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    web_dav_base_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    web_dav_username = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    web_dav_password = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    web_dav_use_basic_auth = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_sources", x => x.id);
                    table.ForeignKey(
                        name: "fk_media_sources_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_media_sources_public_id",
                table: "media_sources",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_media_sources_workspace_id",
                table: "media_sources",
                column: "workspace_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_sources");
        }
    }
}
