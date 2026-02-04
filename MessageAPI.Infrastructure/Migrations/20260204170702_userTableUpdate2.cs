using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessageAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class userTableUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TelegramUserId",
                table: "Users",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramUserId",
                table: "Users");
        }
    }
}
