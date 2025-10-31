using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace M_UserLogin.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveBalancesToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsersId",
                table: "LeaveRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnnualLeaveBalance",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CasualLeaveBalance",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SickLeaveBalance",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_UsersId",
                table: "LeaveRequests",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_AspNetUsers_UsersId",
                table: "LeaveRequests",
                column: "UsersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_AspNetUsers_UsersId",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_UsersId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "AnnualLeaveBalance",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CasualLeaveBalance",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SickLeaveBalance",
                table: "AspNetUsers");
        }
    }
}
