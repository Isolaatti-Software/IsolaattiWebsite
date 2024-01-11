using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.DTOs;
using Isolaatti.Models;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Favorites.Data;

public class FavoritesRepository
{
    private readonly DbContextApp _db;
    public FavoritesRepository(DbContextApp db)
    {
        _db = db;
    }

    public async Task<IEnumerable<FavoriteDto>> GetUserFavorites(int userId)
    {
        var favorites =
            from favorite in _db.Favorites
            from post in _db.SimpleTextPosts
            where favorite.UserId == userId && favorite.PostId == post.Id
            select new FavoriteDto()
            {
                Id = favorite.Id,
                Post = new PostDto()
                {
                    Post = post,
                    UserName = _db.Users.FirstOrDefault(u => u.Id == post.UserId).Name,
                    NumberOfComments = post.Comments.Count,
                    NumberOfLikes = post.Likes.Count,
                    Liked = _db.Likes.Any(l => l.UserId == userId && l.PostId == post.Id),
                    SquadName = post.Squad.Name
                }
            };

        return favorites;

    }

    public async Task<bool> AddToFavorites(long postId, int userId)
    {
        var postCanBeAddedToFavorites = await _db.SimpleTextPosts.AnyAsync(p => p.Privacy != 1 && p.Id == postId);

        if (!postCanBeAddedToFavorites)
        {
            return false;
        }
        
        var favorite = new FavoriteEntity()
        {
            UserId = userId,
            PostId = postId
        };

        await _db.Favorites.AddAsync(favorite);
        return true;
    }
    
    public async Task<bool> RemoveFromFavorites(Guid favoriteId, int userId)
    {
        var favorite = await _db.Favorites.FirstOrDefaultAsync(f => f.Id == favoriteId && f.UserId == userId);

        if (favorite == null)
        {
            return false;
        }

        _db.Favorites.Remove(favorite);
        return true;
    }
}