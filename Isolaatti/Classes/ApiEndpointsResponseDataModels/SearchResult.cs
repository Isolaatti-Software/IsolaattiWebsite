using System.Collections.Generic;
using Isolaatti.DTOs;
using Isolaatti.Models;

namespace Isolaatti.Classes.ApiEndpointsResponseDataModels;

public class SearchResult
{
    public List<UserFeed> Profiles { get; }
    public List<PostDto> Posts { get; }
    public List<Squad> Squads { get; }
    public List<FeedAudio> Audios { get; }
    public List<ImageFeed> Images { get; }

    public SearchResult()
    {
        Profiles = new List<UserFeed>();
        Posts = new List<PostDto>();
        Squads = new List<Squad>();
        Audios = new List<FeedAudio>();
        Images = new List<ImageFeed>();
    }
}