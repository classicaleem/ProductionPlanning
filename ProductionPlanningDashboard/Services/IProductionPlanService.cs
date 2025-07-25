using ProductionPlanningDashboard.Models;
using ProductionPlanningDashboard.Models.ViewModels;

namespace ProductionPlanningDashboard.Services
{
    public interface IProductionPlanService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(FilterViewModel filter);
        Task<DataEntryViewModel> GetDataEntryViewModelAsync(int departmentId, DateTime? date = null);
        Task<int> SaveLineEntriesAsync(int departmentId, DateTime date, List<LineEntry> entries);
        Task<int> SaveCompanyUnitEntriesAsync(int departmentId, DateTime date, List<CompanyUnitEntry> entries);
        Task<List<Line>> GetLinesByCompanyAsync(int companyId, int departmentId);
        Task<List<Unit>> GetUnitsByCompanyAsync(int companyId);
    }
}