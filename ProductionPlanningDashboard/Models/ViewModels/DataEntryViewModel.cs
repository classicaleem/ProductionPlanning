namespace ProductionPlanningDashboard.Models.ViewModels
{
    public class DataEntryViewModel
    {
        public int SelectedDepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public int WeekNumber { get; set; }
        public int MonthNumber { get; set; }
        public int Year { get; set; }
        public string PeriodType { get; set; } = "Weekly";

        // For Full Shoe and Bottom departments
        public List<LineEntry> LineEntries { get; set; } = new();

        // For Upper department
        public List<CompanyUnitEntry> CompanyUnitEntries { get; set; } = new();

        // For backward compatibility with old code
        public List<DataEntryRow> DataRows { get; set; } = new();
        public List<ProductionPlan> ProductionPlans { get; set; } = new();

        // Common master data
        public List<Company> Companies { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
        public List<Line> Lines { get; set; } = new();
        public List<Unit> Units { get; set; } = new();

        // For department switching
        public List<Department> AllDepartments { get; set; } = new();
    }

    public class LineEntry
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public string LineName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public decimal PlannedQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal Difference => ActualQuantity - PlannedQuantity;
        public bool IsNew { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }

    public class CompanyUnitEntry
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
        public decimal PlannedQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal Difference => ActualQuantity - PlannedQuantity;
        public bool IsNew { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }

    // For backward compatibility
    public class DataEntryRow
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int? LineId { get; set; }
        public int? UnitId { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal Difference => ActualQuantity - PlannedQuantity;
        public bool IsNew { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // For display
        public string CompanyName { get; set; } = string.Empty;
        public string LineName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
    }
}