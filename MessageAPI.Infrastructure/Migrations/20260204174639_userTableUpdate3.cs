using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessageAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class userTableUpdate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActivationPin",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PinExpireAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivationPin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PinExpireAt",
                table: "Users");
        }
    }
}
