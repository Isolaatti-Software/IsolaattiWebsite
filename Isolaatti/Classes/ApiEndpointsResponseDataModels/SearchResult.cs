using System.Collections.Generic;
using Isolaatti.Accounts.Data;
using Isolaatti.DTOs;
using Isolaatti.Models;

namespace Isolaatti.Classes.ApiEndpointsResponseDataModels;

public class SearchResult
{
    public List<UserFeedDto> Profiles { get; } = new();
    public List<PostDto> Posts { get; } = new();
    public List<Squad> Squads { get; } = new();
    public List<FeedAudio> Audios { get; } = new();
    public List<ImageFeed> Images { get; } = new();
}