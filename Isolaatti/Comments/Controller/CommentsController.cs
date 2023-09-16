using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.Comments.Entity;
using Isolaatti.Comments.Repository;
using Isolaatti.DTOs;
using Isolaatti.Models;
using Isolaatti.RealtimeInteraction.Service;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Comments.Controller
{
    [ApiController]
    [Route("/api/Comment")]
    public class CommentsController : IsolaattiController
    {
        private readonly DbContextApp _db;
        private readonly NotificationSender _notificationSender;
        private readonly CommentHistoryRepository _commentHistoryRepository;

        public CommentsController(DbContextApp dbContextApp, NotificationSender notificationSender, CommentHistoryRepository commentHistoryRepository)
        {
            _db = dbContextApp;
            _notificationSender = notificationSender;
            _commentHistoryRepository = commentHistoryRepository;
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("/api/Posting/Post/{postId:long}/Comment")]
        public async Task<IActionResult> MakeComment(long postId, MakeCommentModel commentModel)
        {
            var post = await _db.SimpleTextPosts.FindAsync(postId);
            if (post == null)
            {
                return Unauthorized("Post does not exist");
            }

            if (!post.UserId.Equals(User.Id) && post.Privacy == 1)
            {
                return Unauthorized("Post is private");
            }
            var commentToMake = new Comment
            {
                TextContent = commentModel.Content,
                UserId = User.Id,
                PostId = post.Id,
                TargetUser = post.UserId,
                AudioId = commentModel.AudioId,
                SquadId = post.SquadId
            };

            _db.Comments.Add(commentToMake);
            await _db.SaveChangesAsync();

            var commentDto = new CommentDto
            {
                Comment = commentToMake,
                Username = _db.Users.FirstOrDefault(u => u.Id == commentToMake.UserId)?.Name
            };

            try
            {
                var clientId = Guid.Parse(Request.Headers["client-id"]);
                await _notificationSender.SendNewCommentEvent(commentDto, clientId);
            }
            catch (FormatException) { }

            return Ok(commentDto);
        }

        [IsolaattiAuth]
        [HttpGet]
        [Route("/api/Fetch/Post/{postId:long}/Comments")]
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
                    new CommentDto
                    {
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
        [Route("/api/Fetch/Comments/{commentId:long}")]
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
        [Route("Delete")]
        [HttpPost]
        public async Task<IActionResult> Delete(SingleIdentification identification)
        {
            var comment = await _db.Comments.FindAsync(identification.Id);
            if (comment == null) return NotFound("Comment not found");

            if (comment.UserId != User.Id)
            {
                return Unauthorized("Access denied, cannot delete this comment, it is not yours");
            }


            _db.Comments.Remove(comment);
            await _db.SaveChangesAsync();


            try
            {
                var clientId = Guid.Parse(Request.Headers["client-id"]);
                await _notificationSender.SendDeleteCommentEvent(comment.PostId, comment.Id, clientId);
            }
            catch (FormatException) { }


            return Ok();
        }

        [IsolaattiAuth]
        [HttpPost]
        [Route("{commentId:long}/Edit")]
        public async Task<IActionResult> EditComment(long commentId, MakeCommentModel updatedComment)
        {
            var commentToEdit = await _db.Comments.FindAsync(commentId);
            if (commentToEdit == null) return NotFound();
            if (commentToEdit.UserId != User.Id)
            {
                return Unauthorized();
            }

            await _commentHistoryRepository.InsertModificationHistory(commentToEdit);

            commentToEdit.AudioId = updatedComment.AudioId;
            commentToEdit.TextContent = updatedComment.Content;
            commentToEdit.Modified = true;
            commentToEdit.Date = DateTime.UtcNow;

            _db.Comments.Update(commentToEdit);
            await _db.SaveChangesAsync();

            var commentDto = new CommentDto
            {
                Comment = commentToEdit,
                Username = _db.Users.FirstOrDefault(u => u.Id == commentToEdit.UserId)?.Name
            };
            try
            {
                var clientId = Guid.Parse(Request.Headers["client-id"]);
                await _notificationSender.SendCommentModifiedEvent(commentDto, clientId);
            }
            catch (FormatException) { }

            return Ok(commentDto);
        }
    }
}