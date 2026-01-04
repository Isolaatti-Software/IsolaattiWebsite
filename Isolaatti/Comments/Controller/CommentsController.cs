using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Isolaatti.Comments.Dto;
using Isolaatti.Comments.Entity;
using Isolaatti.Comments.Repository;
using Isolaatti.Common;
using Isolaatti.Database.EF;
using Isolaatti.Users.Repository;
using Isolaatti.Utils;
using Isolaatti.Utils.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Comments.Controller;

[ApiController]
[Route("/api/Comments")]
public class CommentsController(
    DbContextApp db,
    CommentHistoryRepository commentHistoryRepository,
    UsersRepository usersRepository) : IsolaattiController
{

    // Use with queries
    private static Expression<Func<Comment, CommentDto>> CommentToDto(int userId)
    {
        return comment => new CommentDto
        {
            Id = comment.Id,
            TextContent = comment.TextContent,
            Username = comment.User.Name,
            Date = comment.Date,
            IsFromUser = comment.UserId == userId,
            Modified = comment.Modified,
            PostId = comment.PostId,
            RepliesCount = comment.Replies.Count,
            ResponseForCommentId = comment.ResponseForCommentId,
            SquadId = comment.SquadId,
            UserId = comment.UserId
        };
    }

    // Use when data has been loaded
    private static CommentDto CommentToDto(int userId, string userName, Comment comment)
    {
        return new CommentDto()
        {
            Id = comment.Id,
            TextContent = comment.TextContent,
            Username = userName,
            Date = comment.Date,
            IsFromUser = comment.UserId == userId,
            Modified = comment.Modified,
            PostId = comment.PostId,
            RepliesCount = comment.Replies.Count,
            ResponseForCommentId = comment.ResponseForCommentId,
            SquadId = comment.SquadId,
            UserId = comment.UserId
        };
    }
    
    [Authorize]
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
            TargetUserId = post.UserId,
            SquadId = post.SquadId,
            IsFromUser = true
        };

        db.Comments.Add(commentToMake);
        await db.SaveChangesAsync();

        var commentDto = CommentToDto(User.Id, User.Name, commentToMake);


        return Ok(commentDto);
    }

    [Authorize]
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

        var commentsList = comments
            .Select(CommentToDto(User.Id))
            .Take(take);
        
        return Ok(new ContentListWrapper<CommentDto>
        {
            Data = commentsList.ToList(),
            MoreContent = total > commentsList.Count()
        });
    }

    [Authorize]
    [HttpGet]
    [Route("{commentId:long}")]
    public async Task<IActionResult> GetComment(long commentId)
    {
        var comment = await db.Comments
            .Where(comment => comment.Id == commentId)
            .Include(comment => comment.User)
            .Select(CommentToDto(User.Id))
            .FirstOrDefaultAsync();
        
        if (comment == null) return NotFound();

        return Ok(new { comment });
    }


    [Authorize]
    [Route("{commentId:long}")]
    [HttpDelete]
    public async Task<IActionResult> Delete([FromRoute] long commentId)
    {
        var comment = await db.Comments.FirstOrDefaultAsync(comment => comment.Id == commentId && comment.UserId == User.Id);
        if (comment == null) return NotFound("Comment not found");
        
        db.Comments.Remove(comment);
        await db.SaveChangesAsync();

        return Ok();
    }

    [Authorize]
    [HttpPut]
    [Route("{commentId:long}")]
    public async Task<IActionResult> EditComment(long commentId, MakeCommentModel updatedComment)
    {
        var commentToEdit = await db.Comments.FindAsync(commentId);
        if (commentToEdit == null) return NotFound();
        if (commentToEdit.UserId != User.Id)
        {
            return Unauthorized();
        }

        //await commentHistoryRepository.InsertModificationHistory(commentToEdit);

        commentToEdit.TextContent = updatedComment.Content;
        commentToEdit.Modified = true;
        commentToEdit.Date = DateTime.UtcNow;
        commentToEdit.IsFromUser = true;

        db.Comments.Update(commentToEdit);
        await db.SaveChangesAsync();

        var commentDto = CommentToDto(User.Id, User.Name, commentToEdit);

        return Ok(commentDto);
    }

    [Authorize]
    [HttpPost]
    [Route("{commentId:long}/reply")]
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
            TargetUserId = post.UserId,
            SquadId = post.SquadId,
            ResponseForCommentId = commentToReply.Id,
            IsFromUser = true
        };

        db.Comments.Add(commentToMake);
        await db.SaveChangesAsync();

        var commentDto = CommentToDto(User.Id, User.Name, commentToMake);

        return Ok(commentDto);
    }
    
    [Authorize]
    [HttpGet]
    [Route("{commentId:long}/replies")]

    public async Task<IActionResult> GetCommentReplies(long commentId, long lastId = long.MaxValue, int take = 10)
    {
        var comments = db.Comments
            .Where(c => c.ResponseForCommentId == commentId && c.Id < lastId)
            .Select(CommentToDto(User.Id))
            .OrderByDescending(c => c.Id)
            .Take(take + 1)
            .ToList();

        var hasMore = comments.Count > take;
        
 
        return Ok(new ContentListWrapper<CommentDto>
        {
            Data = comments,
            MoreContent = hasMore
        });
        
    }
}