namespace ProductionPlanningDashboard.Models
{
    public class Unit
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }
}