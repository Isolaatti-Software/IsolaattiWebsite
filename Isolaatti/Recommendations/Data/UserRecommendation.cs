using System;
using Isolaatti.Accounts.Data.Entity;

namespace Isolaatti.Recommendations.Data;

public class UserRecommendation
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public int RecommendedUserId { get; set; }
    public float CandidateScore { set; get; }
    public User RecommendedUser { get; set; }
}