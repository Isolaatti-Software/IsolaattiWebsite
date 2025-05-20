using System;
using System.Linq;
using System.Threading.Tasks;
using Isolaatti.Comments.Dto;
using Isolaatti.Comments.Entity;
using Isolaatti.Comments.Repository;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.Users.Repository;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Comments.Controller;

[ApiController]
[Route("/api/Comment")]
public class CommentsController(
    DbContextApp db,
    CommentHistoryRepository commentHistoryRepository,
    UsersRepository usersRepository) : IsolaattiController
{
    [IsolaattiAuth]
    [HttpPost]
    [Route("/api/Posting/Post/{postId:long}/Comment")]
    public async Task<IActionResult> MakeComment(long postId, MakeCommentModel commentModel)
    {
        var post = await db.SimpleTextPosts.FindAsync(postId);
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
            SquadId = post.SquadId,
            IsFromUser = true
        };

        db.Comments.Add(commentToMake);
        await db.SaveChangesAsync();

        var commentDto = new CommentDto
        {
            Comment = commentToMake,
            Username = usersRepository.GetUsernameById(commentToMake.UserId) ?? ""
        };


        return Ok(commentDto);
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("/api/Fetch/Post/{postId:long}/Comments")]
    public async Task<IActionResult> GetComments(long postId, long lastId = long.MaxValue, int take = 10)
    {
        var post = await db.SimpleTextPosts.FindAsync(postId);
        if (post == null || (post.Privacy == 1 && post.UserId != User.Id))
            return Unauthorized("post does not exist or is private");

        IQueryable<Comment> comments = db.Comments
            .Where(comment => comment.PostId == post.Id && comment.ResponseForCommentId == null && comment.Id < lastId)
            .OrderByDescending(c => c.Id);

        var total = await comments.CountAsync();

        var commentsList= comments
            .Take(take)
            .ToList();
            
        
            

        return Ok(new ContentListWrapper<CommentDto>
        {
            Data = commentsList.Select(co =>
            {
                co.IsFromUser = User.Id == co.UserId;
                return new CommentDto
                {
                    Comment = co,
                    Username = usersRepository.GetUsernameById(co.UserId) ?? ""
                };
            }).ToList(),
            MoreContent = total > commentsList.Count()
        });
    }

    [IsolaattiAuth]
    [HttpGet]
    [Route("/api/Fetch/Comments/{commentId:long}")]
    public async Task<IActionResult> GetComment(long commentId)
    {
        var comment = await db.Comments.FindAsync(commentId);
        if (comment == null) return NotFound();
        
        comment.IsFromUser = User.Id == comment.UserId;

        return Ok(new CommentDto
        {
            Comment = comment,
            Username = usersRepository.GetUsernameById(comment.UserId) ?? ""
        });
    }


    [IsolaattiAuth]
    [Route("Delete")]
    [HttpPost]
    public async Task<IActionResult> Delete(SingleIdentification identification)
    {
        var comment = await db.Comments.FindAsync(identification.Id);
        if (comment == null) return NotFound("Comment not found");

        if (comment.UserId != User.Id)
        {
            return Unauthorized("Access denied, cannot delete this comment, it is not yours");
        }


        db.Comments.Remove(comment);
        await db.SaveChangesAsync();

        return Ok();
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("{commentId:long}/Edit")]
    public async Task<IActionResult> EditComment(long commentId, MakeCommentModel updatedComment)
    {
        var commentToEdit = await db.Comments.FindAsync(commentId);
        if (commentToEdit == null) return NotFound();
        if (commentToEdit.UserId != User.Id)
        {
            return Unauthorized();
        }

        await commentHistoryRepository.InsertModificationHistory(commentToEdit);

        commentToEdit.TextContent = updatedComment.Content;
        commentToEdit.Modified = true;
        commentToEdit.Date = DateTime.UtcNow;
        commentToEdit.IsFromUser = true;

        db.Comments.Update(commentToEdit);
        await db.SaveChangesAsync();

        var commentDto = new CommentDto
        {
            Comment = commentToEdit,
            Username = usersRepository.GetUsernameById(commentToEdit.UserId) ?? ""
        };

        return Ok(commentDto);
    }

    [IsolaattiAuth]
    [HttpPost]
    [Route("/api/Comments/{commentId:long}/reply")]
    public async Task<IActionResult> ReplyComment(long commentId, MakeCommentModel comment)
    {
        var commentToReply = await db.Comments.FindAsync(commentId);

        if (commentToReply == null)
        {
            return Unauthorized("Comment does not exist");
        }

        var post = await db.SimpleTextPosts.FindAsync(commentToReply.PostId);
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
            TextContent = comment.Content,
            UserId = User.Id,
            PostId = post.Id,
            TargetUser = post.UserId,
            SquadId = post.SquadId,
            ResponseForCommentId = commentToReply.Id,
            IsFromUser = true
        };

        db.Comments.Add(commentToMake);
        await db.SaveChangesAsync();

        var commentDto = new CommentDto
        {
            Comment = commentToMake,
            Username = usersRepository.GetUsernameById(commentToMake.UserId) ?? ""
        };

        return Ok(commentDto);
    }
    
    [IsolaattiAuth]
    [HttpGet]
    [Route("/api/Comments/{commentId:long}/replies")]

    public async Task<IActionResult> GetCommentReplies(long commentId, long lastId = long.MaxValue, int take = 10)
    {
        var comment = await db.Comments.FindAsync(commentId);
        if (comment == null)
        {
            return NotFound("Comment not found");
        }
        IQueryable<Comment> comments = db.Comments
            .Where(c => c.ResponseForCommentId == commentId && c.Id < lastId)
            .OrderByDescending(c => c.Id);
        
        var total = await comments.CountAsync();
        
        var commentsList = comments
            .Take(take)
            .ToList();

        return Ok(new ContentListWrapper<CommentDto>
        {
            Data = commentsList.Select(co =>
            {
                co.IsFromUser = User.Id == co.UserId;
                return new CommentDto
                {
                    Comment = co,
                    Username = usersRepository.GetUsernameById(co.UserId) ?? ""
                };
            }).ToList(),
            MoreContent = total > commentsList.Count
        });
        
    }
}