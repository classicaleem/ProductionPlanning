using ProductionPlanningDashboard.Models.DTOs;

namespace ProductionPlanningDashboard.Models.ViewModels
{
    public class DashboardViewModel
    {
        public FilterViewModel Filters { get; set; } = new();
        public List<SummaryDataDTO> SummaryData { get; set; } = new();
        public List<Company> Companies { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
        public DashboardStats Stats { get; set; } = new();
    }

    public class DashboardStats
    {
        public decimal TotalPlanned { get; set; }
        public decimal TotalActual { get; set; }
        public decimal TotalDifference => TotalActual - TotalPlanned;
        public double PerformancePercentage => TotalPlanned > 0 ? (double)(TotalActual / TotalPlanned * 100) : 0;
        public int TotalRecords { get; set; }
    }
}