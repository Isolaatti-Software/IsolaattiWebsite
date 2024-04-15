using System;
using System.Text.Json.Serialization;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Report.Dto;

namespace Isolaatti.Report.Entity;

public class ReportEntity
{
    public enum ReportStatus
    {
        Open, Resolved, InReview
    }
    public enum ContentTypeEnum
    {
        Post, Comment, Profile, Squad, Picture, Audio
    }
    
    public enum ReportReasonEnum
    {
        Spam, ExplicitContent, ChildRiskContent, Piracy, CopyrightProtectedContent,
        HateAndDiscrimination, IdentityFraud, FraudulentIaGeneratedContent, PeopleInDanger
    }
    
    public Guid Id { get; set; }
    public ReportReasonEnum ReportReason { get; set; }
    public int UserId { get; set; }
    public string UserComment { get; set; }
    public ReportStatus Status { get; set; }
    public string Feedback { get; set; }
    public ContentTypeEnum ContentType { get; set; }
    public string ContentId { get; set; }
    
    [JsonIgnore]
    public User User { get; set; }
}