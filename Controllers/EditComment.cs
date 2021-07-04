using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using isolaatti_API.Utils;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    public class EditComment : ControllerBase
    {
        private readonly DbContextApp Db;

        public EditComment(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        
        [Route("TextContent")]
        [HttpPost]
        public IActionResult TextContent(
            [FromForm] string sessionToken, [FromForm] long commentId, [FromForm] string newContent)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null)
            {
                return Unauthorized("Token is not valid");
            }

            var comment = Db.Comments.Find(commentId);
            if (comment == null) return NotFound("Comment not found");

            if (comment.WhoWrote != user.Id)
            {
                return Unauthorized("Access denied, cannot edit this comment, it is not yours");
            }

            if (string.IsNullOrEmpty(newContent) || string.IsNullOrWhiteSpace(newContent))
            {
                return Unauthorized("Cannot update comment as empty");
            }

            comment.TextContent = newContent;
            Db.Comments.Update(comment);
            
            Db.SaveChanges();
            return Ok("Comment updated successfully");
        }

        [Route("Delete")]
        [HttpPost]
        public IActionResult Delete([FromForm] string sessionToken, [FromForm] long commentId)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null)
            {
                return Unauthorized("Token is not valid");
            }
            
            var comment = Db.Comments.Find(commentId);
            if (comment == null) return NotFound("Comment not found");

            if (comment.WhoWrote != user.Id)
            {
                return Unauthorized("Access denied, cannot delete this comment, it is not yours");
            }
            
            // remove audio if there is any
            if (comment.AudioUrl != null)
            {
                var storage = GoogleCloudBucket.GetInstance();
                storage.DeleteFile(GoogleCloudStorageUrlUtils.GetFileRefFromUrl(comment.AudioUrl));
            }

            Db.Comments.Remove(comment);
            Db.SaveChanges();
            return Ok("Comment delete successfully");
        }

        [Route("ReplaceAudio")]
        [HttpPost]
        public IActionResult ReplaceAudio(
            [FromForm] string sessionToken, [FromForm] long commentId, [FromForm] string newUrl)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null)
            {
                return Unauthorized("Token is not valid");
            }
            
            var comment = Db.Comments.Find(commentId);
            if (comment == null) return NotFound("Comment not found");

            if (comment.WhoWrote != user.Id)
            {
                return Unauthorized("Access denied, cannot replace audio of this comment, it is not yours");
            }
            
            // remove audio if there is any
            if (comment.AudioUrl != null)
            {
                var storage = GoogleCloudBucket.GetInstance();
                storage.DeleteFile(GoogleCloudStorageUrlUtils.GetFileRefFromUrl(comment.AudioUrl));
            }

            if (string.IsNullOrEmpty(newUrl) || string.IsNullOrWhiteSpace(newUrl))
            {
                return Unauthorized("Url cannot be empty");
            }

            comment.AudioUrl = newUrl;
            Db.Comments.Update(comment);
            
            Db.SaveChanges();
            return Ok("Audio replaced");
        }

        [Route("RemoveAudio")]
        [HttpPatch]
        public IActionResult RemoveAudio([FromForm] string sessionToken, [FromForm] long commentId)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null)
            {
                return Unauthorized("Token is not valid");
            }
            
            var comment = Db.Comments.Find(commentId);
            if (comment == null) return NotFound("Comment not found");

            if (comment.WhoWrote != user.Id)
            {
                return Unauthorized("Access denied, cannot replace audio of this comment, it is not yours");
            }
            
            // remove audio if there is any
            if (comment.AudioUrl != null)
            {
                var storage = GoogleCloudBucket.GetInstance();
                storage.DeleteFile(GoogleCloudStorageUrlUtils.GetFileRefFromUrl(comment.AudioUrl));
            }
            
            Db.Comments.Remove(comment);
            
            Db.SaveChanges();
            return Ok("Audio replaced");
        }
    }
}