using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/ZipDownloads")]
    public class ZipDownloads : ControllerBase
    {
        private readonly DbContextApp _db;

        public ZipDownloads(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }

        [Route("data.zip")]
        public IActionResult GetMyPostsAndCommentsZip()
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(Request.Cookies["isolaatti_user_session_token"]);
            if (user == null) return RedirectToPage("LogIn");

            var posts = _db.SimpleTextPosts.Where(post => post.UserId.Equals(user.Id))
                .Select(postEntry => JsonSerializer.Serialize(postEntry, null)).ToArray();
            var comments = _db.Comments.Where(comment => comment.WhoWrote.Equals(user.Id))
                .Select(comment => JsonSerializer.Serialize(comment, null)).ToArray();

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                for (var i = 0; i < posts.Length; i++)
                {
                    var file = Encoding.UTF8.GetBytes(posts[i]);
                    var zipArchiveEntry = archive.CreateEntry($"posts/{i}.json", CompressionLevel.Fastest);
                    using var zipStream = zipArchiveEntry.Open();
                    zipStream.Write(file, 0, file.Length);
                }

                for (var i = 0; i < comments.Length; i++)
                {
                    var file = Encoding.UTF8.GetBytes(comments[i]);
                    var zipArchiveEntry = archive.CreateEntry($"comments/{i}.json", CompressionLevel.Fastest);
                    using var zipStream = zipArchiveEntry.Open();
                    zipStream.Write(file, 0, file.Length);
                }
            }

            return new FileContentResult(memoryStream.ToArray(), "application/zip");
        }
    }
}