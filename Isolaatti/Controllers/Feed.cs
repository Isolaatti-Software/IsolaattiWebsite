using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes;
using Isolaatti.DTOs;
using Isolaatti.Models;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Feed : ControllerBase
    {
        private readonly DbContextApp Db;
        private readonly IAccounts _accounts;

        public Feed(DbContextApp dbContextApp, IAccounts accounts)
        {
            Db = dbContextApp;
            _accounts = accounts;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromHeader(Name = "sessionToken")] string sessionToken, long lastId = 0,
            int length = 10)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            IQueryable<Post> postsQuery;
            
            
            // For now, I am only returning posts that are not from squads
            if (lastId <= 0)
            {
                postsQuery =
                    from post in Db.SimpleTextPosts
                    from following in Db.FollowerRelations
                    where following.UserId == user.Id
                          && post.UserId == following.TargetUserId
                          && post.Privacy != 1
                          && post.SquadId == null
                    orderby post.Id descending 
                    select post;
            }
            else
            {
                postsQuery =
                    from post in Db.SimpleTextPosts
                    from following in Db.FollowerRelations
                    where following.UserId == user.Id
                          && post.UserId == following.TargetUserId
                          && post.Privacy != 1
                          && post.Id < lastId
                          && post.SquadId == null
                    orderby post.Id descending 
                    select post;
            }

            var total = postsQuery.Count();

            postsQuery = postsQuery.Take(length);



            var posts = from post in postsQuery
                select new PostDto
                {
                    Post = post,
                    UserName = Db.Users.FirstOrDefault(u => u.Id == post.Id).Name,
                    NumberOfComments = post.Likes.Count,
                    NumberOfLikes = post.Comments.Count,
                    Liked = Db.Likes.Any(l => l.UserId == user.Id && l.PostId == post.Id),
                    SquadName = post.Squad.Name
                };
            
            
            return Ok(new ContentListWrapper<PostDto>
            {
                Data = posts.ToList(),
                MoreContent = total > length
            });
        }
    }
}