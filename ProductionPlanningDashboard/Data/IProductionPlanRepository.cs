using ProductionPlanningDashboard.Models;
using ProductionPlanningDashboard.Models.DTOs;
using ProductionPlanningDashboard.Models.ViewModels;

namespace ProductionPlanningDashboard.Data
{
    public interface IProductionPlanRepository
    {
        // Master data methods
        Task<List<Company>> GetCompaniesAsync();
        Task<List<Department>> GetDepartmentsAsync();
        Task<List<Line>> GetLinesByCompanyAsync(int companyId, int departmentId);
        Task<List<Unit>> GetUnitsByCompanyAsync(int companyId);
        Task<List<Unit>> GetAllUnitsAsync();

        // Dashboard methods
        Task<List<SummaryDataDTO>> GetSummaryDataAsync(FilterViewModel filter);
        Task<DashboardStats> GetDashboardStatsAsync(FilterViewModel filter);

        // Data entry methods
        Task<List<LineEntry>> GetLineEntriesAsync(int departmentId, DateTime date, string periodType);
        Task<List<CompanyUnitEntry>> GetCompanyUnitEntriesAsync(int departmentId, DateTime date, string periodType);
        Task<int> SaveLineEntriesAsync(int departmentId, DateTime date, string periodType, List<LineEntry> entries);
        Task<int> SaveCompanyUnitEntriesAsync(int departmentId, DateTime date, string periodType, List<CompanyUnitEntry> entries);
        Task<bool> DeleteEntryAsync(int id);
        Task<List<ProductionPlan>> GetProductionPlansAsync(int? departmentId, DateTime? startDate, DateTime? endDate);
    }
}