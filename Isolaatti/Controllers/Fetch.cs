using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Isolaatti.Accounts.Data;
using Isolaatti.Accounts.Data.Entity;
using Isolaatti.Audios.Service;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.Posting.Data;
using Isolaatti.Posting.Dto;
using Isolaatti.Squads.Repository;
using Isolaatti.Users.Repository;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
        private readonly AudiosService _audios;
        private readonly UsersRepository _usersRepository;
        

        public Fetch(DbContextApp dbContextApp, SquadsRepository squadsRepository, AudiosService audios, UsersRepository usersRepository)
        {
            _db = dbContextApp;
            _squads = squadsRepository;
            _audios = audios;
            _usersRepository = usersRepository;
        }

        

        [AllowAnonymous]
        [HttpGet]
        [Route("PostsOfUser/{userId:int}")]
        public async Task<IActionResult> GetPosts(int userId, int length = 30, long lastId = -1, bool olderFirst = false, string? filterJson = null)
        {
            User? requestedAuthor = null;

            PostFilterDto? filter = null;
            
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
                 posts = _db.SimpleTextPosts.Where(post => post.UserId == User.Id && !post.IsDraft);
                 if (filter != null)
                 {
                     posts = filter.IncludeAudio switch
                     {
                         "onlyAudio" => posts.Where(post => post.AudioStoragePath != null),
                         "onlyNoAudio" => posts.Where(post => post.AudioStoragePath == null),
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

                posts = _db.SimpleTextPosts.Where(post => post.UserId == requestedAuthor.Id && post.SquadId == null && post.Privacy != 1 && !post.IsDraft);
                 if (filter != null)
                 {
                     posts = filter.IncludeAudio switch
                     {
                         "onlyAudio" => posts.Where(post => post.AudioStoragePath != null),
                         "onlyNoAudio" => posts.Where(post => post.AudioStoragePath == null),
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


            var a = from post in (posts.Include(p => p.Images))
                from u in _db.Users
                where post.UserId == u.Id
                select new PostDto()
                {
                    Post = post,
                    UserName = u.Name,
                    NumberOfComments = post.Comments.Count,
                    NumberOfLikes = post.Likes.Count,
                    Liked = _db.Likes.Any(l => l.UserId == User.Id && l.PostId == post.Id),
                    SquadName = post.Squad.Name,
                };

            var count = posts.Count();

            var postList = await a.ToListAsync();
            
            return Ok(new ContentListWrapper<PostDto>
            {
                Data = postList,
                MoreContent = total > count
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Post/{postId:long}")]
        public async Task<IActionResult> GetPost(long postId)
        {
            var postExists = await _db.SimpleTextPosts.AnyAsync(p => p.Id == postId && !p.IsDraft);
            if (!postExists)
            {
                return NotFound("post not found");
            }

            var post = _db.SimpleTextPosts
                .Where(p => p.Id == postId)
                .Include(p => p.Images)
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
                if (!await _squads.UserBelongsToSquad(post.Post.SquadId.Value, User.Id))
                {
                    return Unauthorized(new
                    {
                        error = "User does has no access to that post"
                    });
                }
            }

            var participantsInt = (from c in _db.Comments
                where c.PostId == post.Post.Id
                select c.UserId).Distinct();

            var participants = _db.Users
                .Where(u => participantsInt.Contains(u.Id) && u.Id != User.Id)
                .Select(u => new UserFeedDto()
                {
                    Id = u.Id,
                    Name = u.Name,
                    ImageId = u.ProfileImageId,
                    Following = _db.FollowerRelations.Any(fr => fr.UserId == User.Id && fr.TargetUserId == u.Id)
                }).ToList();
            
            post.Participants = participants;
            
            return Ok(post);
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("Post/{postId:long}/LikedBy")]
        public async Task<IActionResult> GetPostLikedBy(long postId)
        {
            
            var post = await _db.SimpleTextPosts.FindAsync(postId);
            if (post == null || (post.Privacy == 1 && post.UserId != User.Id)) return NotFound("post not found");

            var likedBy = (from like in _db.Likes
                from account in _db.Users
                where like.UserId == account.Id && like.PostId == post.Id
                select new UserFeedDto()
                {
                    Id = account.Id,
                    Name = account.Name,
                    ImageId = account.ProfileImageId
                })
                .ToList()
                // this makes another database roundtrip only if this query is not cached
                .Select(u => 
                { 
                    u.Following = _usersRepository.GetUserFollowsUser(User.Id, u.Id); 
                    return u; 
                });

            return Ok(new
            {
                result = likedBy
            });
        }
    }
}