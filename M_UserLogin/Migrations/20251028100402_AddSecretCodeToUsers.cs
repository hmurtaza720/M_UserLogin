using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace M_UserLogin.Migrations
{
    /// <inheritdoc />
    public partial class AddSecretCodeToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecretCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecretCode",
                table: "AspNetUsers");
        }
    }
}
