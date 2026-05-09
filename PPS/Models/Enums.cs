namespace PPS.Models;

public enum EmployeeType
{
    Teaching,
    NonTeaching
}

public enum PayrollType
{
    Monthly,
    HourlyUnit
}

public enum LeaveType
{
    None,
    Sick,
    Vacation,
    Emergency,
    Maternity,
    Paternity
}

public enum PayPeriod
{
    FirstHalf = 1,
    SecondHalf = 2
}

public enum LoanType
{
    SSSLoan,
    PagIBIGLoan,
    CompanyLoan
}

public enum LoanStatus
{
    Active,
    FullyPaid,
    Defaulted
}

public enum UserRole
{
    Admin,
    HR
}

public enum LogSeverity
{
    Info,
    Warning,
    Error
}
