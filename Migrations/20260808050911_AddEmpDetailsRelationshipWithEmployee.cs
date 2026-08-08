using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpDetailsRelationshipWithEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeDetailId",
                schema: "Employee",
                table: "Employee",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EmployeeDetail",
                schema: "Employee",
                columns: table => new
                {
                    EmployeeDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDetail", x => x.EmployeeDetailId);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_EmployeeDetail_EmployeeDetailId",
                schema: "Employee",
                table: "Employee");

            migrationBuilder.DropTable(
                name: "EmployeeDetail",
                schema: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Employee_EmployeeDetailId",
                schema: "Employee",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "EmployeeDetailId",
                schema: "Employee",
                table: "Employee");
        }
    }
}
