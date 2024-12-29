using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPile.DATA.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerifyTokenCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerifyTokenCreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerifyTokenCreatedAt",
                table: "AspNetUsers");
        }
    }
}
