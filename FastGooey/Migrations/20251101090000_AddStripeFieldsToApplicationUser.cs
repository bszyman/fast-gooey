using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastGooey.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeFieldsToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "stripe_customer_id",
                table: "AspNetUsers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stripe_subscription_id",
                table: "AspNetUsers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "subscription_level",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "stripe_customer_id",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "stripe_subscription_id",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "subscription_level",
                table: "AspNetUsers");
        }
    }
}
