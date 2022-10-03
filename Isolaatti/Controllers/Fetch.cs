using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.Classes;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Models;
using Isolaatti.Repositories;
using Isolaatti.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Fetch : ControllerBase
    {
        private readonly DbContextApp _db;
        private readonly IAccounts _accounts;
        private readonly SquadsRepository _squads;

        public Fetch(DbContextApp dbContextApp, IAccounts accounts, SquadsRepository squadsRepository)
        {
            _db = dbContextApp;
            _accounts = accounts;
            _squads = squadsRepository;
        }

        [HttpPost]
        [Route("UserProfile")]
        public async Task<IActionResult> GetProfile([FromHeader(Name = "sessionToken")] string sessionToken,
            SingleIdentification identification)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var account = await _db.Users.FindAsync(Convert.ToInt32(identification.Id));
            if (account == null) return NotFound();

            account.NumberOfFollowers = await _db.FollowerRelations.CountAsync(fr => fr.TargetUserId == account.Id);
            account.NumberOfFollowing = await _db.FollowerRelations.CountAsync(fr => fr.UserId == account.Id);
            account.NumberOfPosts = await _db.SimpleTextPosts.CountAsync(p => p.UserId == account.Id);
            account.NumberOfLikes = await _db.Likes.CountAsync(l => l.TargetUserId == account.Id);
            account.IsUserItself = account.Id == user.Id;
            account.ThisUserIsFollowingMe =
                await _db.FollowerRelations.AnyAsync(fr => fr.TargetUserId == user.Id && fr.UserId == account.Id);
            account.FollowingThisUser =
                await _db.FollowerRelations.AnyAsync(fr => fr.UserId == user.Id && fr.TargetUserId == account.Id);

            
            return Ok(account);
        }

        [HttpGet]
        [Route("PostsOfUser/{userId:int}")]
        public async Task<IActionResult> GetPosts([FromHeader(Name = "sessionToken")] string sessionToken, int userId,
            int length = 30, long lastId = -1, bool olderFirst = false)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            User requestedAuthor = null;

            IQueryable<Post> posts;
            if (user.Id == userId)
            {
                if (olderFirst)
                {
                    if (lastId < 0)
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post => post.UserId == user.Id);
                    }
                    else
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post => post.UserId == user.Id && post.Id > lastId);
                    }
                }
                else
                {
                    if (lastId < 0)
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post => post.UserId == user.Id)
                            .OrderByDescending(post => post.Id);
                    }
                    else
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post => post.UserId == user.Id && post.Id < lastId)
                            .OrderByDescending(post => post.Id);
                    }
                }
            }
            else
            {
                requestedAuthor = await _db.Users.FindAsync(userId);
                if (requestedAuthor == null) return NotFound();

                if (olderFirst)
                {
                    if (lastId < 0)
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post =>
                                post.UserId == requestedAuthor.Id && post.Privacy != 1 && post.SquadId == null)
                            .OrderByDescending(post => post.Id);
                    }
                    else
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post =>
                                post.UserId == requestedAuthor.Id && post.Privacy != 1 && post.Id > lastId &&
                                post.SquadId == null)
                            .OrderByDescending(post => post.Id);
                    }
                }
                else
                {
                    if (lastId < 0)
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post =>
                                post.UserId == requestedAuthor.Id && post.Privacy != 1 && post.SquadId == null)
                            .OrderByDescending(post => post.Id);
                    }
                    else
                    {
                        posts = _db.SimpleTextPosts
                            .Where(post =>
                                post.UserId == requestedAuthor.Id && post.Privacy != 1 && post.Id < lastId &&
                                post.SquadId == null)
                            .OrderByDescending(post => post.Id);
                    }
                }
            }

            var total = posts.Count();
            posts = posts.Take(length);

            var feed =
                (from post in posts
                    select post
                        .SetLiked(_db.Likes.Any(l => l.PostId == post.Id && l.UserId == user.Id))
                        .SetNumberOfComments(_db.Comments.Count(c => c.SimpleTextPostId == post.Id))
                        .SetNumberOfLikes(_db.Likes.Count(l => l.PostId == post.Id))
                        .SetSquadName(_db.Squads.FirstOrDefault(squad => squad.Id.Equals(post.SquadId)).Name)
                        .SetUserName(_accounts.GetUsernameFromId(post.UserId))
                    ).ToList();
            
            


            return Ok(new ContentListWrapper<Post>
            {
                Data = feed,
                MoreContent = total > feed.Count
            });
        }

        [HttpGet]
        [Route("Post/{postId:long}")]
        public async Task<IActionResult> GetPost([FromHeader(Name = "sessionToken")] string sessionToken, long postId)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = await _db.SimpleTextPosts.FindAsync(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != user.Id)) return NotFound("post not found");

            // This post seems to be from a Squad, let's verify user is authorized to see it
            if (post.SquadId != null)
            {
                if (!await _squads.UserBelongsToSquad(user.Id, post.SquadId.Value))
                {
                    return Unauthorized(new
                    {
                        error = "User does has no access to that post"
                    });
                }
            }

            post.Liked = _db.Likes.Any(l => l.PostId == post.Id && l.UserId == user.Id);
            post.NumberOfLikes = await _db.Likes.CountAsync(l => l.PostId == post.Id);
            post.UserName = _accounts.GetUsernameFromId(post.UserId);
            post.SquadName = _db.Squads.FirstOrDefault(squad => squad.Id.Equals(post.SquadId))?.Name;
            post.NumberOfComments = _db.Comments.Count(c => c.SimpleTextPostId == post.Id);
            
            return Ok(post);
        }

        [HttpGet]
        [Route("Post/{postId:long}/Comments/{take:int?}/{lastId:long?}")]
        public async Task<IActionResult> GetComments([FromHeader(Name = "sessionToken")] string sessionToken,
            long postId, long lastId = long.MinValue, int take = 10)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = await _db.SimpleTextPosts.FindAsync(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != user.Id))
                return Unauthorized("post does not exist or is private");

            var comments = _db.Comments
                .Where(comment => comment.SimpleTextPostId.Equals(post.Id) && comment.Id > lastId)
                .OrderBy(c => c.Id)
                .Take(take)
                .ToList();
            return Ok(comments);
        }

        [HttpGet]
        [Route("Comments/{commentId:long}")]
        public async Task<IActionResult> GetComment([FromHeader(Name = "sessionToken")] string sessionToken,
            long commentId)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var comment = await _db.Comments.FindAsync(commentId);
            if (comment == null) return NotFound();

            return Ok(comment);
        }

        [HttpGet]
        [Route("Post/{postId:long}/LikedBy")]
        public async Task<IActionResult> GetPostLikedBy([FromHeader(Name = "sessionToken")] string sessionToken,
            long postId)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var post = await _db.SimpleTextPosts.FindAsync(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != user.Id)) return NotFound("post not found");

            var likedBy =
                from like in _db.Likes
                from account in _db.Users
                where like.UserId == account.Id && like.PostId == post.Id
                select new
                {
                    Id = account.Id,
                    Name = account.Name,
                    ProfileImageId = account.ProfileImageId
                };

            return Ok(likedBy.ToList());
        }
        

        [HttpGet]
        [Route("GetUserProfileImage")]
        public async Task<IActionResult> GetUserProfileImage(int userId)
        {
            var otherUser = await _db.Users.FindAsync(userId);
            if (otherUser == null) return NotFound("User not found");
            if (otherUser.ProfileImageId == null) return Redirect("/res/imgs/user-solid.svg");
            var profileImage = await _db.ProfileImages.FindAsync(otherUser.ProfileImageId);
            if (profileImage == null) return Redirect("/res/imgs/user-solid.svg");
            return new FileContentResult(Convert.FromBase64String(profileImage.ImageData), "image/png");
        }

        [HttpGet]
        [Route("ProfileImages/{id:guid}.png")]
        public async Task<IActionResult> GetProfileImage(Guid id)
        {
            var image = await _db.ProfileImages.FindAsync(id);
            if (image == null) return NotFound();
            if (image.ImageData == null) return NotFound();
            return new FileContentResult(Convert.FromBase64String(image.ImageData), "image/png");
        }

        [HttpGet]
        [Route("ProfileImages/OfUser/{userId:int}")]
        public async Task<IActionResult> GetProfilePhotosOfUser([FromHeader(Name = "sessionToken")] string sessionToken,
            int userId)
        {
            var user = await _accounts.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");

            var images = _db.ProfileImages.Where(image => image.UserId == userId).Select(i => new
            {
                imageId = i.Id,
                relativeUrl = $"/api/Fetch/ProfileImages/{i.Id}.png",
                webEndPoint = $"/imagen/{i.Id}"
            }).ToList();

            return Ok(images);
        }
    }
}