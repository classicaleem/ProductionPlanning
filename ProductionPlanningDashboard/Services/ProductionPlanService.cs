using ProductionPlanningDashboard.Data;
using ProductionPlanningDashboard.Models;
using ProductionPlanningDashboard.Models.ViewModels;

namespace ProductionPlanningDashboard.Services
{
    public class ProductionPlanService : IProductionPlanService
    {
        private readonly IProductionPlanRepository _repository;

        public ProductionPlanService(IProductionPlanRepository repository)
        {
            _repository = repository;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(FilterViewModel filter)
        {
            var viewModel = new DashboardViewModel
            {
                Filters = filter,
                SummaryData = await _repository.GetSummaryDataAsync(filter),
                Companies = await _repository.GetCompaniesAsync(),
                Departments = await _repository.GetDepartmentsAsync(),
                Stats = await _repository.GetDashboardStatsAsync(filter)
            };

            return viewModel;
        }

        public async Task<DataEntryViewModel> GetDataEntryViewModelAsync(int departmentId, DateTime? date = null, string periodType = "Weekly")
        {
            var selectedDate = date ?? DateTime.Today;
            var departments = await _repository.GetDepartmentsAsync();
            var department = departments.FirstOrDefault(d => d.Id == departmentId);
            var companies = await _repository.GetCompaniesAsync();

            var viewModel = new DataEntryViewModel
            {
                SelectedDepartmentId = departmentId,
                DepartmentName = department?.Name ?? "Unknown",
                SelectedDate = selectedDate,
                WeekNumber = GetWeekNumber(selectedDate),
                MonthNumber = selectedDate.Month,
                Year = selectedDate.Year,
                PeriodType = periodType,
                Companies = companies,
                Departments = departments,
                AllDepartments = departments
            };

            // Load department-specific data
            if (department?.Name == "Full Shoe" || department?.Name == "Bottom")
            {
                // Load line entries
                viewModel.LineEntries = await _repository.GetLineEntriesAsync(departmentId, selectedDate, periodType);

                // Convert to DataRows for backward compatibility
                viewModel.DataRows = viewModel.LineEntries.Select(le => new DataEntryRow
                {
                    Id = le.Id,
                    LineId = le.LineId,
                    CompanyName = le.CompanyName,
                    LineName = le.LineName,
                    PlannedQuantity = le.PlannedQuantity,
                    ActualQuantity = le.ActualQuantity,
                    IsNew = le.IsNew,
                    IsDeleted = le.IsDeleted
                }).ToList();
            }
            else if (department?.Name == "Upper")
            {
                // Load company unit entries
                viewModel.CompanyUnitEntries = await _repository.GetCompanyUnitEntriesAsync(departmentId, selectedDate, periodType);
                viewModel.Units = await _repository.GetAllUnitsAsync();

                // Convert to DataRows for backward compatibility
                viewModel.DataRows = viewModel.CompanyUnitEntries.Select(cue => new DataEntryRow
                {
                    Id = cue.Id,
                    CompanyId = cue.CompanyId,
                    UnitId = cue.UnitId,
                    CompanyName = cue.CompanyName,
                    UnitName = cue.UnitName,
                    PlannedQuantity = cue.PlannedQuantity,
                    ActualQuantity = cue.ActualQuantity,
                    IsNew = cue.IsNew,
                    IsDeleted = cue.IsDeleted
                }).ToList();
            }

            // Also populate ProductionPlans for backward compatibility if needed
            try
            {
                viewModel.ProductionPlans = await _repository.GetProductionPlansAsync(departmentId, selectedDate.AddDays(-30), selectedDate.AddDays(30));
            }
            catch
            {
                // If GetProductionPlansAsync doesn't exist, just set empty list
                viewModel.ProductionPlans = new List<ProductionPlan>();
            }

            return viewModel;
        }

        public async Task<int> SaveLineEntriesAsync(int departmentId, DateTime date, string periodType, List<LineEntry> entries)
        {
            return await _repository.SaveLineEntriesAsync(departmentId, date, periodType, entries);
        }

        public async Task<int> SaveCompanyUnitEntriesAsync(int departmentId, DateTime date, string periodType, List<CompanyUnitEntry> entries)
        {
            return await _repository.SaveCompanyUnitEntriesAsync(departmentId, date, periodType, entries);
        }

        public async Task<List<Line>> GetLinesByCompanyAsync(int companyId, int departmentId)
        {
            return await _repository.GetLinesByCompanyAsync(companyId, departmentId);
        }

        public async Task<List<Unit>> GetUnitsByCompanyAsync(int companyId)
        {
            return await _repository.GetUnitsByCompanyAsync(companyId);
        }

        private int GetWeekNumber(DateTime date)
        {
            var jan1 = new DateTime(date.Year, 1, 1);
            var daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;
            var firstThursday = jan1.AddDays(daysOffset);
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}