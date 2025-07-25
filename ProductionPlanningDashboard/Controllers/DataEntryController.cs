using Microsoft.AspNetCore.Mvc;
using ProductionPlanningDashboard.Models.ViewModels;
using ProductionPlanningDashboard.Services;

namespace ProductionPlanningDashboard.Controllers
{
    public class DataEntryController : Controller
    {
        private readonly IProductionPlanService _service;

        public DataEntryController(IProductionPlanService service)
        {
            _service = service;
        }

        // Full Shoe Entry
        public async Task<IActionResult> FullShoe(DateTime? date = null)
        {
            var viewModel = await _service.GetDataEntryViewModelAsync(1, date); // DepartmentId = 1 for Full Shoe
            return View(viewModel);
        }

        // Upper Entry
        public async Task<IActionResult> Upper(DateTime? date = null)
        {
            var viewModel = await _service.GetDataEntryViewModelAsync(2, date); // DepartmentId = 2 for Upper
            return View(viewModel);
        }



        // API Endpoints
        [HttpGet]
        public async Task<IActionResult> GetLinesByCompany(int companyId, int departmentId)
        {
            var lines = await _service.GetLinesByCompanyAsync(companyId, departmentId);
            return Json(lines);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnitsByCompany(int companyId)
        {
            var units = await _service.GetUnitsByCompanyAsync(companyId);
            return Json(units);
        }

        [HttpPost]
        public async Task<IActionResult> SaveFullShoeEntry([FromBody] SaveLineEntryRequest request)
        {
            try
            {
                var savedCount = await _service.SaveLineEntriesAsync(1, request.Date, request.LineEntries);
                return Json(new { success = true, savedCount, message = $"{savedCount} entries saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveBottomEntry([FromBody] SaveLineEntryRequest request)
        {
            try
            {
                var savedCount = await _service.SaveLineEntriesAsync(3, request.Date, request.LineEntries);
                return Json(new { success = true, savedCount, message = $"{savedCount} entries saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveUpperEntry([FromBody] SaveUpperRequest request)
        {
            try
            {
                var savedCount = await _service.SaveCompanyUnitEntriesAsync(2, request.Date, request.CompanyUnitEntries);
                return Json(new { success = true, savedCount, message = $"{savedCount} entries saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class SaveLineEntryRequest
    {
        public DateTime Date { get; set; }
        public string PeriodType { get; set; } = "Weekly";
        public List<LineEntry> LineEntries { get; set; } = new();
    }

    public class SaveUpperRequest
    {
        public DateTime Date { get; set; }
        public string PeriodType { get; set; } = "Weekly";
        public List<CompanyUnitEntry> CompanyUnitEntries { get; set; } = new();
    }
}
