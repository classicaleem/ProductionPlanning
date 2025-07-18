using ProductionPlanningDashboard.Models;
using ProductionPlanningDashboard.Models.ViewModels;

namespace ProductionPlanningDashboard.Services
{
    public interface IProductionPlanService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(FilterViewModel filter);
        Task<DataEntryViewModel> GetDataEntryViewModelAsync(int departmentId, DateTime? date = null, string periodType = "Weekly");
        Task<int> SaveLineEntriesAsync(int departmentId, DateTime date, string periodType, List<LineEntry> entries);
        Task<int> SaveCompanyUnitEntriesAsync(int departmentId, DateTime date, string periodType, List<CompanyUnitEntry> entries);
        Task<List<Line>> GetLinesByCompanyAsync(int companyId, int departmentId);
        Task<List<Unit>> GetUnitsByCompanyAsync(int companyId);
    }
}