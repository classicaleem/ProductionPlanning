namespace ProductionPlanningDashboard.Models.DTOs
{
    public class SummaryDataDTO
    {
        public string CompanyName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime PlanDate { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal Difference { get; set; }
        public string PeriodType { get; set; } = string.Empty;
        public double PerformancePercentage => PlannedQuantity > 0 ? (double)(ActualQuantity / PlannedQuantity * 100) : 0;
        public string PerformanceStatus => PerformancePercentage >= 100 ? "success" : PerformancePercentage >= 80 ? "warning" : "danger";
    }
}