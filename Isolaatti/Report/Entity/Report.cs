using System;
using Isolaatti.Accounts.Data.Entity;

namespace Isolaatti.Report.Entity;

public class Report
{
    public enum ReportStatus
    {
        Open, Resolved, InReview
    }
    public enum ContentTypeEnum
    {
        Post, Comment, Profile, Squad, Picture, Audio
    }
    
    public Guid Id { get; set; }
    public int ReportReasonDescriptionId { get; set; }
    public int UserId { get; set; }
    public string UserComment { get; set; }
    public ReportStatus Status { get; set; }
    public string Feedback { get; set; }
    public ContentTypeEnum ContentType { get; set; }
    public string ContentId { get; set; }
    
    public ReportReasonDescription ReportReasonDescription { get; set; }
    public User User { get; set; }
}