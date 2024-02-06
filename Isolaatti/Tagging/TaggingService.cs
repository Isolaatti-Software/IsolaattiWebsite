using System.Collections.Generic;
using System.Threading.Tasks;
using Isolaatti.Models;
using Isolaatti.Tagging.Entity;
using Isolaatti.Users.Repository;

namespace Isolaatti.Tagging;

public class TaggingService
{
    private readonly DbContextApp _db;
    private readonly UsersRepository _usersRepository;

    public TaggingService(DbContextApp db, UsersRepository usersRepository)
    {
        _db = db;
        _usersRepository = usersRepository;
    }

    private static List<string> _getHashtags(string text)
    {
        // TODO make regex
        return new List<string>();
    }

    private static List<string> _getUserTags(string text)
    {
        // TODO make regex
        return new List<string>();
    }

    public async Task ProcessPost(Post post)
    {
        var hashtags = _getHashtags(post.TextContent);
        var userTags = _getUserTags(post.TextContent);

        var hashtagEntities = new List<HashtagEntity>();
        
        foreach (var hashtag in hashtags)
        {
            var hashtagEntity = new HashtagEntity()
            {
                Text = hashtag.ToLower(),
                PostId = post.Id
            };
            hashtagEntities.Add(hashtagEntity);
        }

        var userTagEntities = new List<UserTagEntity>();

        foreach (var userTag in userTags)
        {
            var userTagEntity = new UserTagEntity()
            {
                PostId = post.Id,
                Username = userTag
            };
            userTagEntities.Add(userTagEntity);
        }
        
        await _db.Hashtags.AddRangeAsync(hashtagEntities);
        await _db.UserTags.AddRangeAsync(userTagEntities);
    }
}