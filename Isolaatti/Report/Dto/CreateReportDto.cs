using Isolaatti.Report.Entity;

namespace Isolaatti.Report.Dto;

public class CreateReportDto
{
    public ReportEntity.ReportReasonEnum Reason { get; set; }
    public string Comment { get; set; }
    public ReportEntity.ContentTypeEnum ContentType { get; set; }
    public string ContentId { get; set; }
}