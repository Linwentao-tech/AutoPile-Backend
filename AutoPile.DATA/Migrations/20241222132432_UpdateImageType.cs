using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPile.DATA.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImageType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Reviews",
                newName: "ImageContentType");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Reviews",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "ImageContentType",
                table: "Reviews",
                newName: "ImageUrl");
        }
    }
}
