using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Tagging.Entity;
using Isolaatti.Users.Repository;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Tagging;

public partial class TaggingService
{
    [GeneratedRegex("#(\\w|-|_)+")]
    private static partial Regex HashtagRegex();

    [GeneratedRegex("@(\\w|-|_)+")]
    private static partial Regex UserTagRegex();
    
    private readonly DbContextApp _db;
    private readonly UsersRepository _usersRepository;

    public TaggingService(DbContextApp db, UsersRepository usersRepository)
    {
        _db = db;
        _usersRepository = usersRepository;
    }

    private static IEnumerable<string> _getHashtags(string text)
    {
        return HashtagRegex().Matches(text).Select(match => match.Value);
    }

    private static IEnumerable<string> _getUserTags(string text)
    {
        return UserTagRegex().Matches(text).Select(match => match.Value);
    }

    public async Task ProcessPost(Post post, bool reset)
    {
        if (reset)
        {
            await _db.Hashtags.Where(ht => ht.PostId == post.Id).ExecuteDeleteAsync();
        }
        
        var hashtags = _getHashtags(post.TextContent);
        var userTags = _getUserTags(post.TextContent);

        var hashtagEntities = 
            hashtags.Select(hashtag => new HashtagEntity()
            {
                Text = hashtag.TrimStart('#').ToLower().Normalize(), PostId = post.Id
            }).ToList();

        var userTagEntities = 
            userTags.Select(userTag => new UserTagEntity()
            {
                PostId = post.Id, Username = userTag
            }).ToList();

        await _db.Hashtags.AddRangeAsync(hashtagEntities);
        await _db.UserTags.AddRangeAsync(userTagEntities);

        await _db.SaveChangesAsync();
    }
}