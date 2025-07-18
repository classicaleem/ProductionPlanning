using System.ComponentModel.DataAnnotations;

namespace ProductionPlanningDashboard.Models
{
    public class ProductionPlan
    {
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        public int? LineId { get; set; }
        public int? UnitId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PlanDate { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Planned quantity must be non-negative")]
        public decimal PlannedQuantity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Actual quantity must be non-negative")]
        public decimal ActualQuantity { get; set; }

        public decimal Difference => ActualQuantity - PlannedQuantity;

        [Required]
        public string PeriodType { get; set; } = "Weekly";

        public int WeekNumber { get; set; }
        public int MonthNumber { get; set; }
        public int Year { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        // Navigation Properties
        public string CompanyName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string LineName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
    }
}