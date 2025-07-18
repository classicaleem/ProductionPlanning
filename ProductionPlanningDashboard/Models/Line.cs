namespace ProductionPlanningDashboard.Models
{
    public class Line
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public int CompanyId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
    }
}