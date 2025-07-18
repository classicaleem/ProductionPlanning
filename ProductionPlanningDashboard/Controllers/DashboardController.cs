using Microsoft.AspNetCore.Mvc;
using ProductionPlanningDashboard.Models.ViewModels;
using ProductionPlanningDashboard.Services;
using System.Text;

namespace ProductionPlanningDashboard.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IProductionPlanService _service;

        public DashboardController(IProductionPlanService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(FilterViewModel filter)
        {
            // Set default dates if not provided
            if (!filter.StartDate.HasValue)
                filter.StartDate = DateTime.Now.AddDays(-30);
            if (!filter.EndDate.HasValue)
                filter.EndDate = DateTime.Now.AddDays(30);

            var viewModel = await _service.GetDashboardDataAsync(filter);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetSummaryData(FilterViewModel filter)
        {
            var viewModel = await _service.GetDashboardDataAsync(filter);
            return PartialView("_SummaryTable", viewModel.SummaryData);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(FilterViewModel filter)
        {
            var viewModel = await _service.GetDashboardDataAsync(filter);

            // Create CSV content
            var csv = new StringBuilder();
            csv.AppendLine("Date,Company,Department,Planned Qty,Actual Qty,Difference,Performance %,Status");

            foreach (var item in viewModel.SummaryData)
            {
                csv.AppendLine($"{item.PlanDate:yyyy-MM-dd},{item.CompanyName},{item.DepartmentName}," +
                             $"{item.PlannedQuantity},{item.ActualQuantity},{item.Difference}," +
                             $"{item.PerformancePercentage:F1},{item.PerformanceStatus}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"ProductionReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(FilterViewModel filter, string chartType = "performance")
        {
            var viewModel = await _service.GetDashboardDataAsync(filter);

            switch (chartType.ToLower())
            {
                case "performance":
                    var performanceData = viewModel.SummaryData
                        .GroupBy(x => x.CompanyName)
                        .Select(g => new
                        {
                            company = g.Key,
                            planned = g.Sum(x => x.PlannedQuantity),
                            actual = g.Sum(x => x.ActualQuantity),
                            performance = g.Average(x => x.PerformancePercentage)
                        });
                    return Json(performanceData);

                case "trend":
                    var trendData = viewModel.SummaryData
                        .GroupBy(x => x.PlanDate.ToString("yyyy-MM"))
                        .Select(g => new
                        {
                            month = g.Key,
                            planned = g.Sum(x => x.PlannedQuantity),
                            actual = g.Sum(x => x.ActualQuantity)
                        })
                        .OrderBy(x => x.month);
                    return Json(trendData);

                case "department":
                    var departmentData = viewModel.SummaryData
                        .GroupBy(x => x.DepartmentName)
                        .Select(g => new
                        {
                            department = g.Key,
                            planned = g.Sum(x => x.PlannedQuantity),
                            actual = g.Sum(x => x.ActualQuantity),
                            efficiency = g.Average(x => x.PerformancePercentage)
                        });
                    return Json(departmentData);

                default:
                    return BadRequest("Invalid chart type");
            }
        }
    }
}