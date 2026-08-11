using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class ChangeParentChildRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employee_EmployeeDetailId",
                schema: "Employee",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "EmployeeDetailId",
                schema: "Employee",
                table: "Employee");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                schema: "Employee",
                table: "EmployeeDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDetail_EmployeeId",
                schema: "Employee",
                table: "EmployeeDetail",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDetail_Employee_EmployeeId",
                schema: "Employee",
                table: "EmployeeDetail",
                column: "EmployeeId",
                principalSchema: "Employee",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeDetail_EmployeeId",
                schema: "Employee",
                table: "EmployeeDetail");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                schema: "Employee",
                table: "EmployeeDetail");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeDetailId",
                schema: "Employee",
                table: "Employee",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_EmployeeDetailId",
                schema: "Employee",
                table: "Employee",
                column: "EmployeeDetailId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_EmployeeDetail_EmployeeDetailId",
                schema: "Employee",
                table: "Employee",
                column: "EmployeeDetailId",
                principalSchema: "Employee",
                principalTable: "EmployeeDetail",
                principalColumn: "EmployeeDetailId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
