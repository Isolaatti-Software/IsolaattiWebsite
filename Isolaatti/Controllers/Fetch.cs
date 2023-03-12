using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.Classes;
using Isolaatti.Models;
using Isolaatti.DTOs;
using Isolaatti.Repositories;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class Fetch : IsolaattiController
    {
        private readonly DbContextApp _db;
        private readonly SquadsRepository _squads;

        public Fetch(DbContextApp dbContextApp, SquadsRepository squadsRepository)
        {
            _db = dbContextApp;
            _squads = squadsRepository;
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("UserProfile/{userId:int}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            
            var account = await _db.Users.FindAsync(userId);
            if (account == null) return NotFound();

            account.NumberOfFollowers = await _db.FollowerRelations.CountAsync(fr => fr.TargetUserId == account.Id);
            account.NumberOfFollowing = await _db.FollowerRelations.CountAsync(fr => fr.UserId == account.Id);
            account.NumberOfPosts = await _db.SimpleTextPosts.CountAsync(p => p.UserId == account.Id);
            account.NumberOfLikes = await _db.Likes.CountAsync(l => l.TargetUserId == account.Id);
            account.IsUserItself = account.Id == User.Id;
            account.ThisUserIsFollowingMe =
                await _db.FollowerRelations.AnyAsync(fr => fr.TargetUserId == User.Id && fr.UserId == account.Id);
            account.FollowingThisUser =
                await _db.FollowerRelations.AnyAsync(fr => fr.UserId == User.Id && fr.TargetUserId == account.Id);

            if (!account.ShowEmail && account.Id != User.Id)
            {
                account.Email = null;
            }

            
            return Ok(account);
        }

        [IsolaattiAuth]
        [HttpGet]
        [Route("PostsOfUser/{userId:int}")]
        public async Task<IActionResult> GetPosts(int userId, int length = 30, long lastId = -1, bool olderFirst = false, string filterJson = null)
        {
            User requestedAuthor = null;

            PostFilterDto filter = null;
            
            // try to parse json filter
            if (filterJson != null)
            {
                filter = JsonSerializer.Deserialize<PostFilterDto>(filterJson, new JsonSerializerOptions() {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
            }
            

            IQueryable<Post> posts;
            if (User.Id == userId)
            {
                /*
                 * Composition of query
                 * 1. Apply filters that are not paging
                 * 2. Order
                 * 3. Paging by cursor
                 */
                 posts = _db.SimpleTextPosts.Where(post => post.UserId == User.Id);
                 if (filter != null)
                 {
                     posts = filter.IncludeAudio switch
                     {
                         "onlyAudio" => posts.Where(post => post.AudioId != null),
                         "onlyNoAudio" => posts.Where(post => post.AudioId == null),
                         _ => posts
                     };

                     posts = filter.IncludeFromSquads switch
                     {
                         "onlyFromSquads" => posts.Where(post => post.SquadId != null),
                         "onlyNotFromSquads" => posts.Where(post => post.SquadId == null),
                         _ => posts
                     };
                     if (filter.DateRange.Enabled)
                     {
                         // try to parse dates

                         var dateFrom = DateTimeOffset.MinValue.ToUniversalTime();
                         var dateTo = DateTimeOffset.MaxValue.ToUniversalTime();

                         try
                         {
                             dateFrom = DateTime.Parse(filter.DateRange.From).ToUniversalTime();
                         }
                         catch (Exception)
                         {
                         }

                         try
                         {
                             dateTo = DateTimeOffset.Parse(filter.DateRange.To).ToUniversalTime();
                         }
                         catch (Exception)
                         {
                         }

                         posts = posts.Where(post => post.Date.Date >= dateFrom && post.Date.Date <= dateTo);
                     }
                 }

                 posts = olderFirst ? posts.OrderBy(post => post.Id) : posts.OrderByDescending(post => post.Id);

                 if (lastId > 0)
                 {
                     posts = olderFirst ? posts.Where(post => post.Id > lastId) : posts.Where(post => post.Id < lastId);
                 }

            }
            else
            {
                requestedAuthor = await _db.Users.FindAsync(userId);
                if (requestedAuthor == null) return NotFound();

                posts = _db.SimpleTextPosts.Where(post => post.UserId == requestedAuthor.Id && post.SquadId == null);
                 if (filter != null)
                 {
                     posts = filter.IncludeAudio switch
                     {
                         "onlyAudio" => posts.Where(post => post.AudioId != null),
                         "onlyNoAudio" => posts.Where(post => post.AudioId == null),
                         _ => posts
                     };
                     
                     if (filter.DateRange.Enabled)
                     {
                         // try to parse dates

                         var dateFrom = DateTime.MinValue;
                         var dateTo = DateTime.MaxValue;

                         try
                         {
                             dateFrom = DateTime.ParseExact(filter.DateRange.From, "yyyy-mm-dd",
                                 CultureInfo.InvariantCulture,
                                 DateTimeStyles.AssumeLocal);
                         }
                         catch (Exception)
                         {
                         }

                         try
                         {
                             dateTo = DateTime.ParseExact(filter.DateRange.To, "yyyy-mm-dd",
                                 CultureInfo.InvariantCulture,
                                 DateTimeStyles.AssumeLocal);
                         }
                         catch (Exception)
                         {
                         }

                         posts = posts.Where(post => post.Date >= dateFrom && post.Date <= dateTo);
                     }
                 }

                 posts = olderFirst ? posts.OrderBy(post => post.Id) : posts.OrderByDescending(post => post.Id);

                 if (lastId > 0)
                 {
                     posts = olderFirst ? posts.Where(post => post.Id > lastId) : posts.Where(post => post.Id < lastId);
                 }
            }

            var total = posts.Count();
            posts = posts.Take(length);


            var a = from post in posts
                from u in _db.Users
                where post.UserId == u.Id
                select new PostDto()
                {
                    Post = post,
                    UserName = u.Name,
                    NumberOfComments = post.Comments.Count,
                    NumberOfLikes = post.Likes.Count,
                    Liked = _db.Likes.Any(l => l.UserId == User.Id && l.PostId == post.Id),
                    SquadName = post.Squad.Name
                };

            var count = posts.Count();


            return Ok(new ContentListWrapper<PostDto>
            {
                Data = a.ToList(),
                MoreContent = total > count
            });
        }

        [IsolaattiAuth]
        [HttpGet]
        [Route("Post/{postId:long}")]
        public async Task<IActionResult> GetPost(long postId)
        {
            var postExists = await _db.SimpleTextPosts.AnyAsync(p => p.Id == postId);
            if (!postExists)
            {
                return NotFound("post not found");
            }

            var post = _db.SimpleTextPosts
                .Where(p => p.Id == postId)
                .Select(p => new PostDto
                {
                    Post = p,
                    UserName = p.User.Name,
                    NumberOfComments = p.Comments.Count,
                    NumberOfLikes = p.Likes.Count,
                    Liked = _db.Likes.Any(l => l.UserId == User.Id && l.PostId == p.Id),
                    SquadName = p.Squad.Name
                })
                .FirstOrDefault();
            

            // This post seems to be from a Squad, let's verify user is authorized to see it
            if (post!.Post.SquadId != null) // at this point post should not be null
            {
                if (!await _squads.UserBelongsToSquad(User.Id, post.Post.SquadId.Value))
                {
                    return Unauthorized(new
                    {
                        error = "User does has no access to that post"
                    });
                }
            }
            
            
            return Ok(post);
        }

        [IsolaattiAuth]
        [HttpGet]
        [Route("Post/{postId:long}/Comments")]
        public async Task<IActionResult> GetComments(long postId, long lastId = long.MinValue, int take = 10)
        {
            var post = await _db.SimpleTextPosts.FindAsync(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != User.Id))
                return Unauthorized("post does not exist or is private");
        
            IQueryable<Comment> comments = _db.Comments
                .Where(comment => comment.PostId.Equals(post.Id) && comment.Id > lastId)
                .OrderBy(c => c.Id);
        
            var total = await comments.CountAsync();
        
            var commentsList = comments
                .Take(take)
                .Select(co => 
                    new CommentDto { 
                        Comment = co, 
                        Username = _db.Users.FirstOrDefault(u => u.Id == co.UserId).Name 
                    })
                .ToList();
        
            return Ok(new ContentListWrapper<CommentDto>
            {
                Data = commentsList,
                MoreContent = total > commentsList.Count
            });
        }

        [IsolaattiAuth]
        [HttpGet]
        [Route("Comments/{commentId:long}")]
        public async Task<IActionResult> GetComment(long commentId)
        {
            var comment = await _db.Comments.FindAsync(commentId);
            if (comment == null) return NotFound();

            return Ok(new CommentDto
            {
                Comment = comment,
                Username = _db.Users.FirstOrDefault(u => u.Id == comment.UserId)?.Name
            });
        }

        [IsolaattiAuth]
        [HttpGet]
        [Route("Post/{postId:long}/LikedBy")]
        public async Task<IActionResult> GetPostLikedBy(long postId)
        {
            
            var post = await _db.SimpleTextPosts.FindAsync(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != User.Id)) return NotFound("post not found");

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
    }
}