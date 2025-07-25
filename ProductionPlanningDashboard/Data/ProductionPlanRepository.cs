using Dapper;
using Microsoft.Data.SqlClient;
using ProductionPlanningDashboard.Models;
using ProductionPlanningDashboard.Models.DTOs;
using ProductionPlanningDashboard.Models.ViewModels;
using System.Data;

namespace ProductionPlanningDashboard.Data
{
    public class ProductionPlanRepository : IProductionPlanRepository
    {
        private readonly string _connectionString;

        public ProductionPlanRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        // ===== MASTER DATA METHODS =====

        public async Task<List<Company>> GetCompaniesAsync()
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM Companies WHERE IsActive = 1 ORDER BY Name";
            var result = await connection.QueryAsync<Company>(sql);
            return result.ToList();
        }

        public async Task<List<Department>> GetDepartmentsAsync()
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM Departments WHERE IsActive = 1 ORDER BY DisplayOrder, Name";
            var result = await connection.QueryAsync<Department>(sql);
            return result.ToList();
        }

        public async Task<List<Line>> GetLinesByCompanyAsync(int companyId, int departmentId)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT l.*, c.Name as CompanyName, d.Name as DepartmentName
                FROM Lines l
                INNER JOIN Companies c ON l.CompanyId = c.Id
                INNER JOIN Departments d ON l.DepartmentId = d.Id
                WHERE l.CompanyId = @companyId 
                  AND l.DepartmentId = @departmentId 
                  AND l.IsActive = 1
                ORDER BY l.DisplayOrder, l.Name";

            var result = await connection.QueryAsync<Line>(sql, new { companyId, departmentId });
            return result.ToList();
        }

        public async Task<List<Unit>> GetUnitsByCompanyAsync(int companyId)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT u.*, c.Name as CompanyName
                FROM Units u
                INNER JOIN Companies c ON u.CompanyId = c.Id
                WHERE u.CompanyId = @companyId AND u.IsActive = 1
                ORDER BY u.Name";

            var result = await connection.QueryAsync<Unit>(sql, new { companyId });
            return result.ToList();
        }

        public async Task<List<Unit>> GetAllUnitsAsync()
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT u.*, c.Name as CompanyName
                FROM Units u
                INNER JOIN Companies c ON u.CompanyId = c.Id
                WHERE u.IsActive = 1
                ORDER BY c.Name, u.Name";

            var result = await connection.QueryAsync<Unit>(sql);
            return result.ToList();
        }

        // ===== DASHBOARD METHODS =====

        public async Task<List<SummaryDataDTO>> GetSummaryDataAsync(FilterViewModel filter)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT 
                    c.Name as CompanyName,
                    d.Name as DepartmentName,
                    pp.PlanDate,
                    pp.PlannedQuantity,
                    pp.ActualQuantity,
                    (pp.ActualQuantity - pp.PlannedQuantity) as Difference                    
                FROM ProductionPlans pp
                INNER JOIN Companies c ON pp.CompanyId = c.Id
                INNER JOIN Departments d ON pp.DepartmentId = d.Id
                WHERE (@CompanyId IS NULL OR pp.CompanyId = @CompanyId)
                  AND (@DepartmentId IS NULL OR pp.DepartmentId = @DepartmentId)
                  AND (@StartDate IS NULL OR pp.PlanDate >= @StartDate)
                  AND (@EndDate IS NULL OR pp.PlanDate <= @EndDate)                  
                ORDER BY pp.PlanDate DESC, c.Name, d.DisplayOrder";
            var result = await connection.QueryAsync<SummaryDataDTO>(sql, filter);
            return result.ToList();
        }

        public async Task<DashboardStats> GetDashboardStatsAsync(FilterViewModel filter)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT 
                    ISNULL(SUM(PlannedQuantity), 0) as TotalPlanned,
                    ISNULL(SUM(ActualQuantity), 0) as TotalActual,
                    COUNT(*) as TotalRecords
                FROM ProductionPlans pp
                WHERE (@CompanyId IS NULL OR pp.CompanyId = @CompanyId)
                  AND (@DepartmentId IS NULL OR pp.DepartmentId = @DepartmentId)
                  AND (@StartDate IS NULL OR pp.PlanDate >= @StartDate)
                  AND (@EndDate IS NULL OR pp.PlanDate <= @EndDate)
                  ";

            var result = await connection.QuerySingleAsync<DashboardStats>(sql, filter);
            return result;
        }

        // ===== DATA ENTRY METHODS =====

        public async Task<List<LineEntry>> GetLineEntriesAsync(int departmentId, DateTime date)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT 
                    pp.Id,
                    pp.LineId,
                    l.Name as LineName,
                    c.Name as CompanyName,
                    pp.PlannedQuantity,
                    pp.ActualQuantity
                FROM ProductionPlans pp
                INNER JOIN Lines l ON pp.LineId = l.Id
                INNER JOIN Companies c ON l.CompanyId = c.Id
                WHERE pp.DepartmentId = @departmentId 
                  AND pp.PlanDate = @date
                  
                  AND pp.LineId IS NOT NULL
                ORDER BY c.Name, l.DisplayOrder";

            var result = await connection.QueryAsync<LineEntry>(sql, new { departmentId, date });
            var entries = result.ToList();

            // Mark existing entries as not new
            foreach (var entry in entries)
            {
                entry.IsNew = false;
            }

            return entries;
        }

        public async Task<List<CompanyUnitEntry>> GetCompanyUnitEntriesAsync(int departmentId, DateTime date)
        {
            using var connection = CreateConnection();

            var sql = @"
                SELECT 
                    pp.Id,
                    pp.CompanyId,
                    c.Name as CompanyName,
                    pp.UnitId,
                    u.Name as UnitName,
                    pp.PlannedQuantity,
                    pp.ActualQuantity
                FROM ProductionPlans pp
                INNER JOIN Companies c ON pp.CompanyId = c.Id
                LEFT JOIN Units u ON pp.UnitId = u.Id
                WHERE pp.DepartmentId = @departmentId 
                  AND pp.PlanDate = @date
                
                  AND pp.UnitId IS NOT NULL
                ORDER BY c.Name, u.Name";

            var result = await connection.QueryAsync<CompanyUnitEntry>(sql, new { departmentId, date });
            var entries = result.ToList();

            // Mark existing entries as not new
            foreach (var entry in entries)
            {
                entry.IsNew = false;
            }

            return entries;
        }

        public async Task<int> SaveLineEntriesAsync(int departmentId, DateTime date, List<LineEntry> entries)
        {
            using var connection = CreateConnection();
            int savedCount = 0;

            foreach (var entry in entries.Where(e => !e.IsDeleted))
            {
                if (entry.IsNew && entry.Id == 0)
                {
                    // Get company from line
                    var companyId = await connection.QuerySingleAsync<int>(
                        "SELECT CompanyId FROM Lines WHERE Id = @LineId",
                        new { entry.LineId });

                    var sql = @"
                        MERGE ProductionPlans AS target
                        USING (SELECT @CompanyId as CompanyId, @DepartmentId as DepartmentId, @LineId as LineId, @PlanDate as PlanDate) AS source
                        ON (target.CompanyId = source.CompanyId AND target.DepartmentId = source.DepartmentId 
                            AND target.LineId = source.LineId AND target.PlanDate = source.PlanDate)
                        WHEN MATCHED THEN 
                            UPDATE SET PlannedQuantity = @PlannedQuantity, ActualQuantity = @ActualQuantity, UpdatedDate = GETDATE()
                        WHEN NOT MATCHED THEN
                            INSERT (CompanyId, DepartmentId, LineId, PlanDate, PlannedQuantity, ActualQuantity,CreatedBy)
                            VALUES (@CompanyId, @DepartmentId, @LineId, @PlanDate, @PlannedQuantity, @ActualQuantity,@CreatedBy);";

                    await connection.ExecuteAsync(sql, new
                    {
                        CompanyId = companyId,
                        DepartmentId = departmentId,
                        entry.LineId,
                        PlanDate = date,
                        entry.PlannedQuantity,
                        entry.ActualQuantity,
                        // PeriodType = periodType,
                        CreatedBy = "System"
                    });

                    savedCount++;
                }
                else if (!entry.IsNew && entry.Id > 0)
                {
                    var sql = @"
                        UPDATE ProductionPlans 
                        SET PlannedQuantity = @PlannedQuantity, 
                            ActualQuantity = @ActualQuantity,
                            UpdatedDate = GETDATE()
                        WHERE Id = @Id";

                    await connection.ExecuteAsync(sql, new
                    {
                        entry.Id,
                        entry.PlannedQuantity,
                        entry.ActualQuantity
                    });

                    savedCount++;
                }
            }

            // Delete marked entries
            foreach (var entry in entries.Where(e => e.IsDeleted && e.Id > 0))
            {
                await DeleteEntryAsync(entry.Id);
            }

            return savedCount;
        }

        public async Task<int> SaveCompanyUnitEntriesAsync(int departmentId, DateTime date, List<CompanyUnitEntry> entries)
        {
            using var connection = CreateConnection();
            int savedCount = 0;

            foreach (var entry in entries.Where(e => !e.IsDeleted))
            {
                if (entry.IsNew && entry.Id == 0)
                {
                    var sql = @"
                        MERGE ProductionPlans AS target
                        USING (SELECT @CompanyId as CompanyId, @DepartmentId as DepartmentId, @UnitId as UnitId, @PlanDate as PlanDate) AS source
                        ON (target.CompanyId = source.CompanyId AND target.DepartmentId = source.DepartmentId 
                            AND target.UnitId = source.UnitId AND target.PlanDate = source.PlanDate)
                        WHEN MATCHED THEN 
                            UPDATE SET PlannedQuantity = @PlannedQuantity, ActualQuantity = @ActualQuantity, UpdatedDate = GETDATE()
                        WHEN NOT MATCHED THEN
                            INSERT (CompanyId, DepartmentId, UnitId, PlanDate, PlannedQuantity, ActualQuantity,CreatedBy)
                            VALUES (@CompanyId, @DepartmentId, @UnitId, @PlanDate, @PlannedQuantity, @ActualQuantity,@CreatedBy);";

                    await connection.ExecuteAsync(sql, new
                    {
                        entry.CompanyId,
                        DepartmentId = departmentId,
                        entry.UnitId,
                        PlanDate = date,
                        entry.PlannedQuantity,
                        entry.ActualQuantity,

                        CreatedBy = "System"
                    });

                    savedCount++;
                }
                else if (!entry.IsNew && entry.Id > 0)
                {
                    var sql = @"
                        UPDATE ProductionPlans 
                        SET PlannedQuantity = @PlannedQuantity, 
                            ActualQuantity = @ActualQuantity,
                            UpdatedDate = GETDATE()
                        WHERE Id = @Id";

                    await connection.ExecuteAsync(sql, new
                    {
                        entry.Id,
                        entry.PlannedQuantity,
                        entry.ActualQuantity
                    });

                    savedCount++;
                }
            }

            // Delete marked entries
            foreach (var entry in entries.Where(e => e.IsDeleted && e.Id > 0))
            {
                await DeleteEntryAsync(entry.Id);
            }

            return savedCount;
        }

        public async Task<bool> DeleteEntryAsync(int id)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM ProductionPlans WHERE Id = @id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { id });
            return rowsAffected > 0;
        }

        public async Task<List<ProductionPlan>> GetProductionPlansAsync(int? departmentId, DateTime? startDate, DateTime? endDate)
        {
            using var connection = CreateConnection();

            var sql = @"
        SELECT 
            pp.Id,
            pp.CompanyId,
            pp.DepartmentId,
            pp.LineId,
            pp.UnitId,
            pp.PlanDate,
            pp.PlannedQuantity,
            pp.ActualQuantity,
            pp.PeriodType,
            pp.CreatedDate,
            pp.UpdatedDate,
            pp.CreatedBy,
            c.Name as CompanyName,
            d.Name as DepartmentName,
            ISNULL(l.Name, '') as LineName,
            ISNULL(u.Name, '') as UnitName,
            DATEPART(WEEK, pp.PlanDate) as WeekNumber,
            MONTH(pp.PlanDate) as MonthNumber,
            YEAR(pp.PlanDate) as Year
        FROM ProductionPlans pp
        INNER JOIN Companies c ON pp.CompanyId = c.Id
        INNER JOIN Departments d ON pp.DepartmentId = d.Id
        LEFT JOIN Lines l ON pp.LineId = l.Id
        LEFT JOIN Units u ON pp.UnitId = u.Id
        WHERE (@DepartmentId IS NULL OR pp.DepartmentId = @DepartmentId)
          AND (@StartDate IS NULL OR pp.PlanDate >= @StartDate)
          AND (@EndDate IS NULL OR pp.PlanDate <= @EndDate)
        ORDER BY pp.PlanDate DESC, c.Name";

            var result = await connection.QueryAsync<ProductionPlan>(sql, new { departmentId, startDate, endDate });
            return result.ToList();
        }

    }
}