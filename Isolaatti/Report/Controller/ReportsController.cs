using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Database.EF;
using Isolaatti.Report.Dto;
using Isolaatti.Report.Entity;
using Isolaatti.Utils;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Report.Controller;

[ApiController]
[Route("/api/reports")]
public class ReportsController(DbContextApp db) : ProfileRequiredController
{
    [HttpPost]
    
    public async Task<IActionResult> CreateReport([FromBody] CreateReportDto createReportDto)
    {
        var entity = new ReportEntity()
        {
            ReportReason = createReportDto.Reason,
            UserId = Profile.Id,
            Status = ReportEntity.ReportStatus.Open,
            Feedback = "",
            ContentType = createReportDto.ContentType,
            ContentId = createReportDto.ContentId,
            UserComment = createReportDto.Comment
        };
        db.Reports.Add(entity);
        await db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpGet]
    
    [Route("by_me")]
    public async Task<IActionResult> GetReportsByUser()
    {
        var reports = await db.Reports.Where(report => report.UserId == Profile.Id).ToListAsync();

        return Ok(new
        {
            result = reports
        });
    }
}