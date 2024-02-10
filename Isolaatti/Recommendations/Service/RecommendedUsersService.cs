using System.Threading.Tasks;
using Isolaatti.Models;

namespace Isolaatti.Recommendations.Service;

public class RecommendedUsersService
{
    private const float LikeWeight = 0.2f;
    private const float FollowWeight = 0.3f;
    
    private readonly DbContextApp _db;

    public RecommendedUsersService(DbContextApp db)
    {
        _db = db;
    }

    public async Task OnLike(int userId, long postId)
    {
        
    }

    public async Task OnLikeRemove(int userId, long postId)
    {
        
    }

    public async Task OnFollow(int userId, int followedUserId)
    {
        
    }
    
    public async Task OnFollowRemove(int userId, int followedUserId)
    {
        
    }

    public async Task OnViewLessContent(int userId, int targetUserId)
    {
        
    }
}