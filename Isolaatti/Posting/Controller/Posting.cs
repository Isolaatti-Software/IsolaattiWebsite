using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Audios.Service;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.Posting.Data;
using Isolaatti.Posting.Dto;
using Isolaatti.RealtimeInteraction.Service;
using Isolaatti.Services;
using Isolaatti.Squads.Repository;
using Isolaatti.Tagging;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Posting.Controller
{
    [ApiController]
    [Route("/api/Posting")]
    public class PostingController(
        DbContextApp _db, 
        NotificationSender _notificationSender, 
        SquadsRepository _squads, 
        TaggingService _taggingService,
        AudiosService _audios,
        GoogleCloudStorageService cloudStorage) : IsolaattiController
    {
        
        [HttpPost]
        [Route("Make")]
        public async Task<IActionResult> Index(MakePostModel post)
        {
            // Let's verify if Squad exists and that user is authorized to post
            if (post.SquadId.HasValue)
            {
                var userBelongs = await _squads.UserBelongsToSquad(post.SquadId.Value, User.Id);
                if (!userBelongs)
                {
                    return NotFound(new
                    {
                        error = "Squad does not exist"
                    });
                }
            }


            // Yep, here I can create the post
            var newPost = new Post()
            {
                UserId = User.Id,
                TextContent = post.Content,
                SquadId = post.SquadId,
                Privacy = post.Privacy,
                IsDraft = post.IsDraft
            };
            
            _db.SimpleTextPosts.Add(newPost);
            await _db.SaveChangesAsync();

            // extract and store hashtags and user tags
            await _taggingService.ProcessPost(newPost, false);


            return Ok(new PostDto()
            {
                Post = newPost,
                UserName = _db.Users.FirstOrDefault(u => u.Id == newPost.UserId)?.Name,
                NumberOfComments = 0,
                NumberOfLikes = 0,
                Liked = false,
                SquadName = newPost.Squad?.Name,
            });
        }

        [HttpPost]
        [Route("Edit")]
        public async Task<IActionResult> EditPost(EditPostModel editedPost)
        {
            var existingPost = await _db.SimpleTextPosts.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == editedPost.PostId);
            if (existingPost == null)
            {
                return NotFound("Post not found");
            }
            if (existingPost.UserId != User.Id)
            {
                return Unauthorized("Post is not yours, cannot edit");
            }
            _db.PostVersions.Add(new PostVersionEntity()
            {
                PostId = existingPost.Id,
                TextContent = existingPost.TextContent,
                DateTime = DateTime.Now.ToUniversalTime(),
            });

            existingPost.Privacy = editedPost.Privacy;
            existingPost.TextContent = editedPost.Content;
            existingPost.IsDraft = editedPost.IsDraft;

            // relate images to post
            await _db.Images
                .Where(i => editedPost.Images.Contains(i.Id) && i.UserId == User.Id)
                .ForEachAsync(i =>  i.PostId = existingPost.Id);

            

            if (editedPost.RemoveAudio && existingPost.AudioStoragePath != null)
            {
                await _audios.DeleteAudio(existingPost.AudioStoragePath);
                existingPost.AudioStoragePath = null;
                existingPost.AudioDurationMs = 0;
            }
            
            _db.SimpleTextPosts.Update(existingPost);
            await _db.SaveChangesAsync();

            await _taggingService.ProcessPost(existingPost, true);

            var updatedPost = new PostDto()
            {
                Post = existingPost,
                UserName = _db.Users.FirstOrDefault(u => u.Id == existingPost.UserId)?.Name ?? "",
                NumberOfComments = await _db.Comments.CountAsync(c => c.PostId == existingPost.Id),
                NumberOfLikes = await _db.Likes.CountAsync(l => l.PostId == existingPost.Id),
                Liked = _db.Likes.Any(l => l.UserId == User.Id && l.PostId == existingPost.Id),
                SquadName = existingPost.SquadId == null ? null : _squads.GetSquadName(existingPost.SquadId)
            };

            var success = Guid.TryParse(Request.Headers["client-id"], out var clientId);
            if (success)
            {
                _notificationSender.SendPostUpdate(editedPost.PostId, clientId);
            }

            return Ok(updatedPost);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IActionResult> DeletePost(SingleIdentification identification)
        {
            var post = await _db.SimpleTextPosts.FindAsync(identification.Id);
            if (post == null)
            {
                return NotFound();
            }

            if (!post.UserId.Equals(User.Id))
            {
                return Unauthorized("You cannot delete a post that is not yours");
            }
                
        
            // Deleting related items
            var commentsOfPost = _db.Comments.Where(comment => comment.PostId == post.Id).ToList();
            _db.Comments.RemoveRange(commentsOfPost);
            
            var likesOfPost = _db.Likes.Where(like => like.PostId == post.Id).ToList();
            _db.Likes.RemoveRange(likesOfPost);
        
            // Here I can delete the post
            _db.SimpleTextPosts.Remove(post);
            
            await _db.SaveChangesAsync();
        
            return Ok(new DeletePostOperationResult()
            {
                PostId = post.Id,
                Success = true,
                OperationTime = DateTime.Now
            });
        }

        [HttpGet]
        [Route("Post/{postId:long}/Versions")]
        public async Task<IActionResult> GetPostVersions(long postId)
        {
            var versions = await _db.PostVersions
                .Where(pv => pv.PostId == postId)
                .OrderByDescending(pv => pv.Id)
                .ToListAsync();
            return Ok(new {result = versions});
        }

        [HttpPost]
        [Route("Post/{postId:long}/SetImages")]
        public async Task<IActionResult> SetImages(long postId, ListPayload<Guid> imagesIds)
        {
            // check existence and ownership of post
            var post = await _db.SimpleTextPosts.AnyAsync(p => p.Id == postId && p.UserId == User.Id);

            if (!post)
            {
                return NotFound("Post not found or ownership check failed");
            }
            
            // specified images that are indeed owned by the user
            var images = _db.Images.Where(i => imagesIds.Items.Contains(i.Id) && i.UserId == User.Id).ToList();
            
            foreach (var image in images)
            {
                image.PostId = postId;
            }
            
            _db.Images.UpdateRange(images);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Route("Post/{postId:long}/SetIsDraft")]
        public async Task<IActionResult> SetIsDraft(long postId, [FromQuery] bool isDraft)
        {
            // check existence and ownership of post
            var post = await _db.SimpleTextPosts.FirstOrDefaultAsync(p => p.Id == postId && p.UserId == User.Id);

            if (post == null)
            {
                return NotFound("Post not found or ownership check failed");
            }

            post.IsDraft = isDraft;

            _db.SimpleTextPosts.Update(post);
            await _db.SaveChangesAsync();

            return Ok();
        }


        [HttpPost]
        [Authorize]
        [Route("Post/{postId:long}/Audio")]
        public async Task<IActionResult> SetAudio([FromForm] IFormFile audioFile, [FromRoute] long postId, [FromForm] int duration)
        {
            await _audios.CreateAudio(audioFile.OpenReadStream(), User.Id, postId, audioFile.ContentType, duration);

            return Ok();
        }

        [HttpGet]
        [Authorize]
        [Route("Post/{postId:long}/DownloadAudio")]
        public async Task<IActionResult> DownloadAudio(long postId)
        {
            var post = _db.SimpleTextPosts.FirstOrDefault(p => p.Id == postId);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            if (post.AudioStoragePath == null)
            {
                return NotFound("Post has no audio");
            }

            return Redirect(await cloudStorage.GetDownloadUrl(post.AudioStoragePath));
        }
    }
}