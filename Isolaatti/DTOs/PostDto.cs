using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Models;

namespace Isolaatti.DTOs;

public class PostDto
{
    public Post Post { get; set; }
    
    public int NumberOfLikes { get; set; }
    public int NumberOfComments { get; set; }
    public string UserName { get; set; }
    public string SquadName { get; set; }
    public bool Liked { get; set; }
    public FeedAudio? Audio { get; set; }
}