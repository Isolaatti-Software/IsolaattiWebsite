using Isolaatti.Comments.Dto;
using Isolaatti.Comments.Repository;
using Isolaatti.Models;
using Isolaatti.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Isolaatti.Comments.Controller
{
    public class CommentHistoryController : IsolaattiController
    {
        private readonly DbContextApp _db;
        private readonly CommentHistoryRepository _commentHistoryRepository;

        public CommentHistoryController(DbContextApp dbContextApp, CommentHistoryRepository commentHistoryRepository)
        {
            _db = dbContextApp;
            _commentHistoryRepository = commentHistoryRepository;
        }

        [HttpGet]
        [Route("/api/Comment/{commentId:long}/History")]
        public async Task<IActionResult> GetCommentHistory(long commentId)
        {
            var commentExists = await _db.Comments.AnyAsync(c => c.Id == commentId);
            if (!commentExists)
            {
                return NotFound();
            }

            var modificationHistoryEntries = await _commentHistoryRepository.GetModificationHistory(commentId);

            var comments = modificationHistoryEntries.Select(m => m.Comment);

            return Ok(new CommentHistory { CommentId = commentId, Comments = comments.ToList() });


        }
    }
}
