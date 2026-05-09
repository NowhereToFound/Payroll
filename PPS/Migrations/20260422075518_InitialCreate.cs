using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PPS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmployeeType = table.Column<int>(type: "int", nullable: false),
                    PayrollType = table.Column<int>(type: "int", nullable: false),
                    BasicMonthlySalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SSSNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TINNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhilHealthNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PagIBIGNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BiometricId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DateHired = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                });

            migrationBuilder.CreateTable(
                name: "SystemLogs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Severity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLogs", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    AttendanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    TimeIn = table.Column<TimeOnly>(type: "time", nullable: true),
                    TimeOut = table.Column<TimeOnly>(type: "time", nullable: true),
                    OvertimeHours = table.Column<decimal>(type: "decimal(7,2)", nullable: false),
                    LeaveType = table.Column<int>(type: "int", nullable: false),
                    IsAbsent = table.Column<bool>(type: "bit", nullable: false),
                    LateMinutes = table.Column<decimal>(type: "decimal(7,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.AttendanceId);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoanRecords",
                columns: table => new
                {
                    LoanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    LoanType = table.Column<int>(type: "int", nullable: false),
                    LoanAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyAmortization = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanRecords", x => x.LoanId);
                    table.ForeignKey(
                        name: "FK_LoanRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRecords",
                columns: table => new
                {
                    PayrollId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    PayYear = table.Column<int>(type: "int", nullable: false),
                    PayMonth = table.Column<int>(type: "int", nullable: false),
                    PayPeriod = table.Column<int>(type: "int", nullable: false),
                    PayPeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PayPeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    BasicPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OvertimePay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TardinessDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AbsenceDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SSS_Employee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhilHealth_Employee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PagIBIG_Employee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SSS_Employer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhilHealth_Employer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PagIBIG_Employer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxableIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WithholdingTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoanDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPosted = table.Column<bool>(type: "bit", nullable: false),
                    Is13thMonth = table.Column<bool>(type: "bit", nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRecords", x => x.PayrollId);
                    table.ForeignKey(
                        name: "FK_PayrollRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoanPayments",
                columns: table => new
                {
                    LoanPaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanId = table.Column<int>(type: "int", nullable: false),
                    PayrollId = table.Column<int>(type: "int", nullable: true),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanPayments", x => x.LoanPaymentId);
                    table.ForeignKey(
                        name: "FK_LoanPayments_LoanRecords_LoanId",
                        column: x => x.LoanId,
                        principalTable: "LoanRecords",
                        principalColumn: "LoanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "FullName", "IsActive", "LastLoginAt", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "System Administrator", true, null, "$2a$11$K9Do.8L3kOdFTKTK5v7GKuXGsHyEeZNYWJTl9SWpCn0MIXSwmblWu", 0, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_EmployeeId_Date",
                table: "AttendanceRecords",
                columns: new[] { "EmployeeId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanPayments_LoanId",
                table: "LoanPayments",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRecords_EmployeeId",
                table: "LoanRecords",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_EmployeeId_PayYear_PayMonth_PayPeriod_Is13thMonth",
                table: "PayrollRecords",
                columns: new[] { "EmployeeId", "PayYear", "PayMonth", "PayPeriod", "Is13thMonth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_Severity",
                table: "SystemLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_Timestamp",
                table: "SystemLogs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceRecords");

            migrationBuilder.DropTable(
                name: "LoanPayments");

            migrationBuilder.DropTable(
                name: "PayrollRecords");

            migrationBuilder.DropTable(
                name: "SystemLogs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "LoanRecords");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
