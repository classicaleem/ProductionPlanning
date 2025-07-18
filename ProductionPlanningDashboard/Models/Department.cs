namespace ProductionPlanningDashboard.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
    }
}