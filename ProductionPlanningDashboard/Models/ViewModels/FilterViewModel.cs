namespace ProductionPlanningDashboard.Models.ViewModels
{
    public class FilterViewModel
    {
        public int? CompanyId { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PeriodType { get; set; } = "Weekly";
    }
}