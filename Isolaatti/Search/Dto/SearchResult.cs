using System.Collections.Generic;
using Isolaatti.Accounts.Data;
using Isolaatti.Images.Data;
using Isolaatti.Posting.Dto;
using Isolaatti.Squads.Data;

namespace Isolaatti.Search.Dto;

public class SearchResult
{
    public List<UserFeedDto> Profiles { get; } = new();
    public List<PostDto> Posts { get; } = new();
    public List<Squad> Squads { get; } = new();
    public List<ImageFeed> Images { get; } = new();
}