using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsRequestDataModels;
using Isolaatti.DTOs;
using Isolaatti.Models;
using Isolaatti.Services;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Isolaatti.Controllers
{
    [ApiController]
    [Route("/api/Comment")]
    public class CommentsController : IsolaattiController
    {
        private readonly DbContextApp _db;
        private readonly NotificationSender _notificationSender;

        public CommentsController(DbContextApp dbContextApp, NotificationSender notificationSender)
        {
            _db = dbContextApp;
            _notificationSender = notificationSender;
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
            } catch(FormatException) {}
            
            
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
            commentToEdit.AudioId = updatedComment.AudioId;
            commentToEdit.TextContent = updatedComment.Content;
        
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
            } catch(FormatException) {}

            return Ok(commentDto);
        }
    }
}