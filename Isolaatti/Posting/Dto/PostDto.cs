using System.Collections.Generic;
using Isolaatti.Accounts.Data;
using Isolaatti.Posting.Data;

namespace Isolaatti.Posting.Dto;

public class PostDto
{
    public Post Post { get; set; }
    
    public int NumberOfLikes { get; set; }
    public int NumberOfComments { get; set; }
    public string UserName { get; set; }
    public string? SquadName { get; set; }
    public bool Liked { get; set; }
    public List<UserFeedDto>? Participants { get; set; }
}